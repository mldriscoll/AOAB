using System.IO.Compression;
using System.IO;
using OBB_WPF.Library;

namespace OBB_WPF.Editor
{
    public static class Unpacker
    {
        public static Task Unpack(Series series)
        {
            if (!Directory.Exists(series.InternalName)) Directory.CreateDirectory(series.InternalName);

            foreach (var volume in series.Volumes.Where(x => File.Exists($"{Settings.Configuration.SourceFolder}\\{x.FileName}")))
            {
                var directory = $"{series.InternalName}\\{volume.ApiSlug}";
                if (!Directory.Exists(directory)) ZipFile.ExtractToDirectory($"{Settings.Configuration.SourceFolder}\\{volume.FileName}", directory);
                else
                {
                    var finfo = new FileInfo($"{Settings.Configuration.SourceFolder}\\{volume.FileName}");
                    var dinfo = new DirectoryInfo(directory);
                    if (dinfo.CreationTimeUtc < finfo.CreationTimeUtc)
                    {
                        Directory.Delete(directory);
                        ZipFile.ExtractToDirectory($"{Settings.Configuration.SourceFolder}\\{volume.FileName}", directory);
                    }
                }
            }

            foreach (var volume in series.Volumes.Where(x => File.Exists($"{x.FileName}")))
            {
                var directory = $"{series.InternalName}\\{volume.ApiSlug}";
                if (!Directory.Exists(directory)) ZipFile.ExtractToDirectory($"{volume.FileName}", directory);
                else
                {
                    var finfo = new FileInfo($"{volume.FileName}");
                    var dinfo = new DirectoryInfo(directory);
                    if (dinfo.CreationTimeUtc < finfo.CreationTimeUtc)
                    {
                        Directory.Delete(directory);
                        ZipFile.ExtractToDirectory($"{volume.FileName}", directory);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
