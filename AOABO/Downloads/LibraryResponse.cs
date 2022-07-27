namespace AOABO.Downloads
{
    public class LibraryResponse
    {
        public List<Book> books { get; set; }

        public class Book
        {
            public Volume volume { get; set; }

            public class Volume
            {
                public string slug { get; set; }
            }

            public string lastDownload { get; set; }
            public string lastUpdated { get; set; }

            public List<Download> downloads { get; set; }

            public class Download
            {
                public string link { get; set; }
                public string label { get; set; }
            }
        }
    }
}