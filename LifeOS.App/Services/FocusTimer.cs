using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LifeOS.App.Services
{
    /// <summary>
    /// Focus timer shared by the whole app. It counts down to a real end time,
    /// so it stays correct while you move between pages. Finished (or stopped) focus time
    /// is saved as a FocusSession, and also as study / skill hours when linked to one.
    /// </summary>
    public sealed class FocusTimer : IDisposable
    {
        private readonly System.Threading.Timer _tick;
        private readonly IServiceScopeFactory _scopes;

        public FocusTimer(IServiceScopeFactory scopes)
        {
            _scopes = scopes;
            _tick = new System.Threading.Timer(_ => OnTick(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public int TotalSeconds { get; private set; } = 25 * 60;
        public int RemainingSeconds { get; private set; } = 25 * 60;
        public bool IsRunning { get; private set; }
        public bool IsOpen { get; private set; }
        public bool JustFinished { get; private set; }
        public string Label { get; set; } = "";

        /// <summary>Free | Task | ProjectTask | StudyTask | StudySubject | Skill</summary>
        public string Kind { get; private set; } = "Free";
        public int? RefId { get; private set; }

        private DateTime? _endAt;
        private DateTime? _startedAt;

        /// <summary>Focused seconds not yet saved (a finished block is saved at once, so it counts 0).</summary>
        public int ElapsedSeconds => JustFinished ? 0 : TotalSeconds - RemainingSeconds;

        public event Action? Changed;
        /// <summary>Raised after a focus block was saved (message for a toast).</summary>
        public event Action<string>? Logged;

        public string Display => $"{RemainingSeconds / 60:00}:{RemainingSeconds % 60:00}";
        public double Fraction => TotalSeconds == 0 ? 0 : (double)RemainingSeconds / TotalSeconds;

        public void Toggle() { IsOpen = !IsOpen; Changed?.Invoke(); }
        public void Close() { IsOpen = false; Changed?.Invoke(); }

        /// <summary>Links the timer to something (only while it is not running).</summary>
        public void SetTarget(string kind, int? refId, string label)
        {
            if (IsRunning) return;
            // Hours already counted for another target must not move to the new one.
            if (ElapsedSeconds > 0) _ = SaveAsync(ElapsedSeconds);
            _startedAt = null;
            RemainingSeconds = TotalSeconds;
            Kind = kind; RefId = refId; Label = label;
            JustFinished = false;
            Changed?.Invoke();
        }

        public void SetMinutes(int minutes)
        {
            minutes = Math.Clamp(minutes, 1, 300);
            Pause();
            if (ElapsedSeconds > 0) _ = SaveAsync(ElapsedSeconds);
            _startedAt = null;
            TotalSeconds = RemainingSeconds = minutes * 60;
            JustFinished = false;
            Changed?.Invoke();
        }

        public void Start()
        {
            if (RemainingSeconds <= 0) RemainingSeconds = TotalSeconds;
            _startedAt ??= DateTime.Now;
            _endAt = DateTime.UtcNow.AddSeconds(RemainingSeconds);
            IsRunning = true;
            JustFinished = false;
            _tick.Change(250, 250);
            Changed?.Invoke();
        }

        public void Pause()
        {
            _tick.Change(Timeout.Infinite, Timeout.Infinite);
            IsRunning = false;
            _endAt = null;
            Changed?.Invoke();
        }

        public void Reset()
        {
            Pause();
            int elapsed = ElapsedSeconds;
            RemainingSeconds = TotalSeconds;
            JustFinished = false;
            if (elapsed > 0) _ = SaveAsync(elapsed);
            _startedAt = null;
            Changed?.Invoke();
        }

        private void OnTick()
        {
            if (_endAt == null) return;
            var left = (int)Math.Ceiling((_endAt.Value - DateTime.UtcNow).TotalSeconds);
            RemainingSeconds = Math.Max(0, left);
            if (RemainingSeconds == 0)
            {
                _tick.Change(Timeout.Infinite, Timeout.Infinite);
                IsRunning = false;
                _endAt = null;
                JustFinished = true;
                IsOpen = true;
                _ = SaveAsync(TotalSeconds);
                _startedAt = null;
            }
            Changed?.Invoke();
        }

        /// <summary>Saves a focus block. Blocks under one minute are ignored.</summary>
        private async Task SaveAsync(int seconds)
        {
            int minutes = seconds / 60;
            if (minutes < 1) return;
            var started = _startedAt ?? DateTime.Now.AddMinutes(-minutes);
            string kind = Kind, label = Label.Trim();
            int? refId = RefId;
            try
            {
                using var scope = _scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.FocusSessions.Add(new FocusSession
                {
                    StartedAt = started, Minutes = minutes, Kind = kind, RefId = refId,
                    Label = label.Length == 0 ? null : (label.Length > 200 ? label[..200] : label),
                });
                double hours = Math.Round(minutes / 60.0, 2);
                if (kind == "StudySubject" && refId is int sid && await db.StudySubjects.FindAsync(sid) != null)
                    db.StudySessions.Add(new StudySession { StudySubjectId = sid, SessionDate = DateTime.Today, DurationHours = hours, TopicCovered = "Focus timer", FocusRating = 7 });
                else if (kind == "Skill" && refId is int kid && await db.TechSkills.FindAsync(kid) != null)
                    db.TechSkillLogs.Add(new TechSkillLog { TechSkillId = kid, LogDate = DateTime.Today, Hours = hours, TopicCovered = "Focus timer" });
                await db.SaveChangesAsync();
                Logged?.Invoke($"Focus saved: {minutes} min");
            }
            catch
            {
                // Never let a save problem break the timer.
            }
        }

        public void Dispose() => _tick.Dispose();
    }
}
