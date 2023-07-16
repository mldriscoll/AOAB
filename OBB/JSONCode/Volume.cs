﻿namespace OBB.JSONCode
{
    public class Volume
    {
        public string InternalName { get; set; } = string.Empty;

        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
        public List<Chapter> BonusChapters { get; set; } = new List<Chapter>();
        public List<Chapter> ExtraContent { get; set; } = new List<Chapter>();
        public List<Gallery> Gallery { get; set; } = new List<Gallery> { new JSONCode.Gallery() };
    }
}
