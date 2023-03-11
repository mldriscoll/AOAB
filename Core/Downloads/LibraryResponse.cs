namespace Core.Downloads
{
    public class LibraryResponse
    {
        public List<Book> Books { get; set; } = new List<Book>();

        public class Book
        {
            public VolumeResponse Volume { get; set; } = new VolumeResponse();

            public class VolumeResponse
            {
                public string Slug { get; set; } = string.Empty;
            }

            public string LastDownload { get; set; } = String.Empty;
            public string LastUpdated { get; set; } = String.Empty;

            public List<Download> Downloads { get; set; } = new List<Download>();

            public class Download
            {
                public string Link { get; set; } = String.Empty;
                public string Label { get; set; } = String.Empty;
            }
        }
    }
}
