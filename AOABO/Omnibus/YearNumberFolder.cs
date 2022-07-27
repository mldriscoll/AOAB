using AOABO.Omnibus;

public class YearNumberFolder : IFolder
{
    public string MakeFolder(string folder, int zero, int year)
    {
        return folder.Replace("[Year]", $"{101 + year}-{(zero + year)}");
    }
}
