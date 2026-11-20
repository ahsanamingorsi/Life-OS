namespace LifeOS.App.Services
{
    public static class AppPaths
    {
        /// <summary>Where backups are saved. LIFEOS_BACKUP_DIR overrides it (used for testing).</summary>
        public static string BackupDirectory =>
            Environment.GetEnvironmentVariable("LIFEOS_BACKUP_DIR") is { Length: > 0 } dir
                ? dir
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LifeOS Backups");

        /// <summary>Where lifeos.db lives. LIFEOS_DATA_DIR overrides it (used for testing, so real data is never touched).</summary>
        public static string DataDirectory
        {
            get
            {
                var overrideDir = Environment.GetEnvironmentVariable("LIFEOS_DATA_DIR");
                if (!string.IsNullOrWhiteSpace(overrideDir))
                {
                    Directory.CreateDirectory(overrideDir);
                    return overrideDir;
                }
                return FileSystem.AppDataDirectory;
            }
        }
    }
}
