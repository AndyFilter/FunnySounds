using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FunnySounds
{
    public class Utils
    {
        public static MediaPlayer? PlayAudio(string path, float volume = 0.5f)
        {
            var del = App.Current.Dispatcher.BeginInvoke(new Func<MediaPlayer>(() =>
            {
                var player = new MediaPlayer();
                player.MediaEnded += new EventHandler(Player_MediaEnded);
                player.Volume = Math.Pow(volume, 1.5f);
                player.Open(new Uri(path));
                player.Play();
                return player;
            }
            ));
            del.Wait();
            MediaPlayer? returnPlayer = del.Result as MediaPlayer;
            if (returnPlayer != null)
            {
                return returnPlayer;
            }

            return null;
        }

        private static void Player_MediaEnded(object? sender, EventArgs e)
        {
            if (sender != null && sender is System.Windows.Media.MediaPlayer mediaPlayer)
            {
                mediaPlayer.Close();
            }
        }

        //GET THE ACTUAL EXTENSION OF THE FILE! some frick might use m4a, or ogg, or wav 0_0 sussy
        public static async Task<Structs.Sound?> DownloadAudio(string downloadUri, string fileName)
        {
            var filePath = Path.Combine(Structs.dataDir, Path.GetFileName(downloadUri));
            if (File.Exists(filePath)) return new Structs.Sound() { name = fileName, path = filePath }; ;
            using (var webClient = new WebClient())
            {
                try
                {
                    await webClient.DownloadFileTaskAsync(new Uri(downloadUri), Path.Combine(downloadUri, filePath));
                    return new Structs.Sound() { name = fileName, path = filePath };
                }
                catch (Exception)
                {
                    MessageBox.Show("There was an error while downloading the song");
                    return null;
                }
            }
        }

        public static Structs.UserData? GetUserData()
        {
            if (!Directory.Exists(Structs.dataDir))
            {
                Directory.CreateDirectory(Structs.dataDir);
                return null;
            }

            if (!File.Exists(Path.Combine(Structs.dataDir, "UserData.json")))
            {
                return null;
            }
            else
            {
                var fileText = File.ReadAllText(Path.Combine(Structs.dataDir, "UserData.json"));
                var UserData = JsonSerializer.Deserialize<Structs.UserData>(fileText);

                if (UserData != null)
                    return UserData;
                else
                    return null;
            }
        }

        public static void SaveUserData(Structs.UserData userData)
        {
            if (!Directory.Exists(Structs.dataDir))
            {
                Directory.CreateDirectory(Structs.dataDir);
            }

            var dataText = JsonSerializer.Serialize<Structs.UserData>(userData);
            File.WriteAllText(Path.Combine(Structs.dataDir, "UserData.json"), dataText);
        }

        public static void GetSounds(Structs.UserData userData, ref List<Structs.Sound> soundsList)
        {
            foreach (var sound in userData.sounds)
            {
                if (sound != null)
                {
                    soundsList.Add(sound);
                }
            }
        }
    }
}
