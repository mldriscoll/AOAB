namespace OBB
{
    public class MiscSettings
    {
        public bool UseHumanReadableFileNames { get; set; } = false;
        public bool RemoveTempFolder { get; set; } = true;
        public bool DownloadBooks { get; set; } = true;
        public string? InputFolder { get; set; } = "Downloads";
        public string GetInputFolder()
        {
            return Settings.MiscSettings.InputFolder == null ? Environment.CurrentDirectory :
                Settings.MiscSettings.InputFolder.Length > 1 && Settings.MiscSettings.InputFolder[1].Equals(':') ? Settings.MiscSettings.InputFolder : Environment.CurrentDirectory + "\\" + Settings.MiscSettings.InputFolder;
        }
        public string? OutputFolder { get; set; }

        public static string FolderDisplay(string? folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                return "[current folder]";

            if (folder.Length > 1 && folder[1].Equals(':'))
            {
                return folder;
            }

            return $"[current folder]\\{folder}";
        }
    }
}
