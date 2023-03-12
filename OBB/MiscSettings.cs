namespace OBB
{
    public class MiscSettings
    {
        public bool UseHumanReadableFileNames { get; set; } = false;
        public bool RemoveTempFolder { get; set; } = true;
        public bool DownloadBooks { get; set; } = true;
        public string? InputFolder { get; set; } = "Downloads";
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
