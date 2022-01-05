using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FunnySounds
{
    public class Structs
    {
        public static string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FunnySounds");
        public static string[] AUDIO_FILE_EXTENSIONS = {".mp3", ".wav", ".ogg", ".m4a", ".flac", ".aac"};
        public class Sound
        {
            public string name { get; set; } = "";
            public string path { get; set; } = "";
            public float odds { get; set; } = 0.5f;
            public float volume { get; set; } = 0.5f;
            public bool isEnabled { get; set; } = true;
        }

        [Serializable]
        public class UserData //I've made a class from this data in case I want to add some more parameters to it in the future.
        {
            public List<Sound> sounds { get; set; } = new List<Sound>();
            public float globalFrequency { get; set; } = 0.5f;
            public float globalVolume { get; set; } = 1f;
        }
    }
}