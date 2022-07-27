using AOABO.Omnibus;

public class YearFolder : IFolder
{
    public string MakeFolder(string folder, int zero, int year)
    {
        return folder.Replace("[Year]", $"{101 + year}-Year {(zero + year)}");
    }
}
