using System.Runtime.Serialization.Json;

namespace AOABO.Config
{
    public static class Configuration
    {
        public static readonly List<Volume> Volumes;
        public static readonly List<VolumeName> VolumeNames;

        static Configuration()
        {
            using (var reader = new StreamReader("Volumes.json"))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Volume[]));
                Volumes = ((Volume[])deserializer.ReadObject(reader.BaseStream)).ToList();
            }
            using (var reader = new StreamReader("VolumeNames.json"))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(VolumeName[]));
                VolumeNames = ((VolumeName[])deserializer.ReadObject(reader.BaseStream)).ToList();
            }
        }
    }
}