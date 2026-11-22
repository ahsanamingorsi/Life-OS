using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using LifeOS.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Sync
{
    public class SyncStats
    {
        public int Inserted, Updated, Deleted;
        public int Total => Inserted + Updated + Deleted;
        public override string ToString() => $"{Inserted} new, {Updated} updated, {Deleted} deleted";
    }

    /// <summary>
    /// Turns the whole database into a snapshot and merges a snapshot from another device.
    /// Every record has a global id (SyncId) and a "last changed" time; the newest change wins.
    /// </summary>
    public static class SyncEngine
    {
        // ── helpers ───────────────────────────────────────────────────────
        private static List<object> Rows(DbContext db, Type type)
        {
            var method = typeof(DbContext).GetMethods()
                .First(m => m.Name == "Set" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0)
                .MakeGenericMethod(type);
            return ((IEnumerable)method.Invoke(db, null)!).Cast<object>().ToList();
        }

        private static Guid SyncIdOf(DbContext db, object o) => (Guid)db.Entry(o).Property(SyncCatalog.SyncIdProp).CurrentValue!;
        private static long UpdatedOf(DbContext db, object o) => (long)db.Entry(o).Property(SyncCatalog.UpdatedProp).CurrentValue!;
        private static int IdOf(DbContext db, object o) => (int)db.Entry(o).Property("Id").CurrentValue!;

        private static IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IProperty> DataProps(DbContext db, Type type) =>
            db.Model.FindEntityType(type)!.GetProperties().Where(p => !p.IsShadowProperty() && !p.IsPrimaryKey());

        private static Type? PrincipalOf(Microsoft.EntityFrameworkCore.Metadata.IProperty p) =>
            p.GetContainingForeignKeys().FirstOrDefault()?.PrincipalEntityType.ClrType;

        // ── give old rows an id ───────────────────────────────────────────
        /// <summary>Rows created before sync existed get an id and a time. Safe to run on every start.</summary>
        public static async Task EnsureStampsAsync(AppDbContext db)
        {
            db.SuppressSyncStamps = true;
            long now = DateTime.UtcNow.Ticks;
            bool any = false;
            foreach (var type in SyncCatalog.Ordered)
                foreach (var o in Rows(db, type))
                {
                    var entry = db.Entry(o);
                    if ((Guid)entry.Property(SyncCatalog.SyncIdProp).CurrentValue! != Guid.Empty) continue;
                    entry.Property(SyncCatalog.SyncIdProp).CurrentValue = Guid.NewGuid();
                    entry.Property(SyncCatalog.UpdatedProp).CurrentValue = now;
                    any = true;
                }
            if (any) await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            db.SuppressSyncStamps = false;
        }

        // ── export ────────────────────────────────────────────────────────
        public static async Task<JsonObject> ExportAsync(AppDbContext db)
        {
            var ids = new Dictionary<(Type, int), Guid>();
            var tables = new JsonObject();

            foreach (var type in SyncCatalog.Ordered)
            {
                var props = DataProps(db, type).ToList();
                var poly = SyncCatalog.Polymorphic.TryGetValue(type, out var p) ? p : default;
                bool isPoly = poly.KindProp != null;
                var array = new JsonArray();

                foreach (var o in Rows(db, type))
                {
                    var sid = SyncIdOf(db, o);
                    if (sid == Guid.Empty) continue;
                    ids[(type, IdOf(db, o))] = sid;

                    var row = new JsonObject { ["_id"] = sid.ToString(), ["_u"] = UpdatedOf(db, o) };
                    bool skip = false;
                    foreach (var prop in props)
                    {
                        if (isPoly && prop.Name == poly.IdProp) continue;
                        var value = prop.PropertyInfo!.GetValue(o);
                        var principal = PrincipalOf(prop);
                        if (principal != null)
                        {
                            if (value == null) row[prop.Name + "@"] = null;
                            else if (ids.TryGetValue((principal, Convert.ToInt32(value)), out var parent)) row[prop.Name + "@"] = parent.ToString();
                            else { skip = true; break; }
                        }
                        else row[prop.Name] = JsonSerializer.SerializeToNode(value, prop.ClrType);
                    }
                    if (skip) continue;

                    if (isPoly)
                    {
                        var kind = type.GetProperty(poly.KindProp)!.GetValue(o) as string;
                        var refId = type.GetProperty(poly.IdProp)!.GetValue(o);
                        var target = SyncCatalog.TypeForKind(kind);
                        row["_ref"] = refId != null && target != null && ids.TryGetValue((target, Convert.ToInt32(refId)), out var rg) ? rg.ToString() : null;
                    }
                    array.Add(row);
                }
                tables[type.Name] = array;
            }

            var tombs = new JsonArray();
            foreach (var t in await db.SyncTombstones.AsNoTracking().ToListAsync())
                tombs.Add(new JsonObject { ["t"] = t.EntityType, ["id"] = t.SyncId.ToString(), ["at"] = t.DeletedTicks });

            return new JsonObject { ["v"] = 1, ["tables"] = tables, ["tombstones"] = tombs };
        }

        // ── merge ─────────────────────────────────────────────────────────
        public static async Task<SyncStats> MergeAsync(AppDbContext db, JsonObject snapshot)
        {
            var stats = new SyncStats();
            db.SuppressSyncStamps = true;
            try
            {
                db.ChangeTracker.Clear();
                var tombs = (await db.SyncTombstones.ToListAsync()).ToDictionary(t => (t.EntityType, t.SyncId));

                // 1) deletions made on the other device
                foreach (var node in snapshot["tombstones"]?.AsArray() ?? new JsonArray())
                {
                    string typeName = node!["t"]!.GetValue<string>();
                    var id = Guid.Parse(node["id"]!.GetValue<string>());
                    long at = node["at"]!.GetValue<long>();
                    var type = SyncCatalog.Ordered.FirstOrDefault(x => x.Name == typeName);
                    if (type == null) continue;

                    if (tombs.TryGetValue((typeName, id), out var existing)) { if (existing.DeletedTicks < at) existing.DeletedTicks = at; }
                    else
                    {
                        var rec = new Sync.SyncTombstone { EntityType = typeName, SyncId = id, DeletedTicks = at };
                        db.SyncTombstones.Add(rec);
                        tombs[(typeName, id)] = rec;
                    }

                    var row = Rows(db, type).FirstOrDefault(r => SyncIdOf(db, r) == id);
                    if (row != null && UpdatedOf(db, row) <= at) { db.Remove(row); stats.Deleted++; }
                }
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();

                // 2) new and changed records, parents first
                var loaded = new Dictionary<Type, List<object>>();
                var maps = new Dictionary<Type, Dictionary<Guid, int>>();
                Dictionary<Guid, int> MapOf(Type t)
                {
                    if (!maps.TryGetValue(t, out var m))
                    {
                        m = new Dictionary<Guid, int>();
                        foreach (var o in loaded.TryGetValue(t, out var l) ? l : Rows(db, t))
                            m[SyncIdOf(db, o)] = IdOf(db, o);
                        maps[t] = m;
                    }
                    return m;
                }

                var tables = snapshot["tables"]?.AsObject();
                foreach (var type in SyncCatalog.Ordered)
                {
                    var remote = tables?[type.Name]?.AsArray();
                    if (remote == null || remote.Count == 0) continue;

                    var local = Rows(db, type);
                    loaded[type] = local;
                    var bySync = new Dictionary<Guid, object>();
                    foreach (var o in local) bySync[SyncIdOf(db, o)] = o;

                    SyncCatalog.NaturalKeys.TryGetValue(type, out var keyProps);
                    string? KeyOf(Func<string, object?> get) =>
                        keyProps == null ? null : string.Join("|", keyProps.Select(k => get(k) is DateTime d ? d.ToString("yyyyMMdd") : get(k)?.ToString()));
                    var byKey = new Dictionary<string, object>();
                    if (keyProps != null)
                        foreach (var o in local)
                            byKey[KeyOf(n => type.GetProperty(n)!.GetValue(o))!] = o;

                    var props = DataProps(db, type).ToList();
                    var hasPoly = SyncCatalog.Polymorphic.TryGetValue(type, out var poly);

                    foreach (var node in remote)
                    {
                        var r = node!.AsObject();
                        var guid = Guid.Parse(r["_id"]!.GetValue<string>());
                        long u = r["_u"]!.GetValue<long>();
                        if (tombs.TryGetValue((type.Name, guid), out var tomb) && tomb.DeletedTicks >= u) continue;

                        // read values, mapping parent ids to this device's ids
                        var values = new Dictionary<string, object?>();
                        bool ok = true;
                        foreach (var prop in props)
                        {
                            if (hasPoly && prop.Name == poly.IdProp) continue;
                            var principal = PrincipalOf(prop);
                            if (principal != null)
                            {
                                var s = r[prop.Name + "@"]?.GetValue<string>();
                                if (s == null)
                                {
                                    if (prop.ClrType == typeof(int)) { ok = false; break; }
                                    values[prop.Name] = null;
                                }
                                else if (MapOf(principal).TryGetValue(Guid.Parse(s), out var localId)) values[prop.Name] = localId;
                                else { ok = false; break; }
                            }
                            else
                            {
                                var node2 = r[prop.Name];
                                values[prop.Name] = node2 == null ? (prop.ClrType.IsValueType && Nullable.GetUnderlyingType(prop.ClrType) == null ? Activator.CreateInstance(prop.ClrType) : null)
                                    : node2.Deserialize(prop.ClrType);
                            }
                        }
                        if (!ok) continue;

                        if (hasPoly)
                        {
                            var kind = values[poly.KindProp] as string;
                            var target = SyncCatalog.TypeForKind(kind);
                            var refGuid = r["_ref"]?.GetValue<string>();
                            if (target != null && refGuid != null && MapOf(target).TryGetValue(Guid.Parse(refGuid), out var rid)) values[poly.IdProp] = rid;
                            else { values[poly.IdProp] = null; values[poly.KindProp] = "Free"; }
                        }

                        bySync.TryGetValue(guid, out var match);
                        string? key = KeyOf(n => values.TryGetValue(n, out var v) ? v : null);
                        if (match == null && key != null) byKey.TryGetValue(key, out match);

                        void Apply(object target)
                        {
                            foreach (var (name, value) in values) type.GetProperty(name)!.SetValue(target, value);
                        }

                        if (match == null)
                        {
                            var created = Activator.CreateInstance(type)!;
                            Apply(created);
                            db.Add(created);
                            var entry = db.Entry(created);
                            entry.Property(SyncCatalog.SyncIdProp).CurrentValue = guid;
                            entry.Property(SyncCatalog.UpdatedProp).CurrentValue = u;
                            bySync[guid] = created;
                            local.Add(created);
                            if (key != null) byKey[key] = created;
                            stats.Inserted++;
                        }
                        else
                        {
                            long localU = UpdatedOf(db, match);
                            var entry = db.Entry(match);
                            bool adopt = SyncIdOf(db, match) != guid;
                            if (u > localU)
                            {
                                Apply(match);
                                entry.Property(SyncCatalog.UpdatedProp).CurrentValue = u;
                                stats.Updated++;
                            }
                            if (adopt)
                            {
                                entry.Property(SyncCatalog.SyncIdProp).CurrentValue = guid;
                                bySync[guid] = match;
                            }
                        }
                    }

                    await db.SaveChangesAsync();
                    maps.Remove(type);
                }
                return stats;
            }
            finally
            {
                db.ChangeTracker.Clear();
                db.SuppressSyncStamps = false;
            }
        }
    }
}
