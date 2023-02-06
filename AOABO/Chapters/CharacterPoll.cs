using AOABO.Config;

namespace AOABO.Chapters
{
    public class CharacterPoll : Chapter
    {
        protected override string GetFlatSubFolder()
        {
            return Configuration.FolderNames["CharacterPolls"];
        }

        protected override string GetPartSubFolder()
        {
            return Configuration.FolderNames["CharacterPolls"];
        }

        protected override string GetVolumeSubFolder()
        {
            return Configuration.FolderNames["CharacterPolls"];
        }

        protected override string GetYearsSubFolder()
        {
            return Configuration.FolderNames["CharacterPolls"];
        }
    }

}