using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace OBB_WPF
{
    public class Source : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string File { get; set; } = string.Empty;

        public ObservableCollection<string> Alternates { get; set; } = new ObservableCollection<string>();

        public Source? OtherSide { get; set; } = null;

        public string SortOrder { get; set; } = string.Empty;

        [JsonIgnore]
        public string LeftURI
        {
            set {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("LeftURI"));
            }
            get
            {
                if (OtherSide == null) return "about:blank";

                if (System.IO.File.Exists(OtherSide.File)) return OtherSide.File;

                foreach (var alt in OtherSide.Alternates)
                    if (System.IO.File.Exists(alt)) return alt;

                return "about:blank";
            }
        }

        [JsonIgnore] 
        public string RightURI
        {
            get
            {
                if (System.IO.File.Exists(File)) return File;

                foreach (var alt in Alternates)
                    if (System.IO.File.Exists(alt)) return alt;

                return "about:blank";
            }
        }
    }
}