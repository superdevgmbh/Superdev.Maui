using Foundation;

namespace Superdev.Maui.Platforms.iOS.Utils
{
    /// <summary>
    /// This helper allows to disable iTunes/iCloud backup for specific app folders.
    /// See also: https://developer.apple.com/library/archive/documentation/FileManagement/Conceptual/FileSystemProgrammingGuide/FileSystemOverview/FileSystemOverview.html
    /// </summary>
    public static class NSFileManagerHelper
    {
        public static void DisableBackup()
        {
            NSFileManager.SetSkipBackupAttribute(DocumentsFolder, skipBackup: true);
            NSFileManager.SetSkipBackupAttribute(CachesFolder, skipBackup: true);
            NSFileManager.SetSkipBackupAttribute(PreferencesFolder, skipBackup: true);
        }

        private static string DocumentsFolder => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private static string CachesFolder => Path.Combine(DocumentsFolder, "..", "Library", "Caches");

        private static string PreferencesFolder => Path.Combine(DocumentsFolder, "..", "Library", "Preferences");
    }
}