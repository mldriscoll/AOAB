namespace Core.Downloads
{
    public class LibraryResponse
    {
        public List<Book> books { get; set; } = new List<Book>();

        public class Book
        {
            public VolumeResponse volume { get; set; } = new VolumeResponse();

            public class VolumeResponse
            {
                public string slug { get; set; } = string.Empty;
            }

            public string lastDownload { get; set; } = String.Empty;
            public string lastUpdated { get; set; } = String.Empty;

            public List<Download> downloads { get; set; } = new List<Download>();

            public class Download
            {
                public string link { get; set; } = String.Empty;
                public string label { get; set; } = String.Empty;
            }
        }
    }
}
