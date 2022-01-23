using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace FunnySounds
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //randomize volume / chance to randomize volume?
        private string dataDir = Structs.dataDir;
        private WebClient webClient = new WebClient();
        private Timer playerTimer = new Timer();
        private List<Structs.Sound> availableSounds = new List<Structs.Sound>();
        private Structs.UserData userData = new Structs.UserData();
        private float globalOdds = 0.5f; //Used to balance the requency of the playAudioTick function. So its more like a frequency
        private float globalVolume = 1f;
        private Random rnd = new Random();

        private Structs.Sound? droppedSoundFile;
        public MainWindow()
        {
            InitializeComponent();

            var _UserData = Utils.GetUserData();

            if (_UserData != null)
            {
                userData = _UserData;
                globalFrequencySlider.Value = Math.Pow(userData.globalFrequency, (1 / 1.5f));
                globalVolumeSlider.Value = userData.globalVolume;
            }
            else
                userData = new Structs.UserData();

            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(Structs.dataDir);
                FirstTimeSetup();
            }
            if (!File.Exists(Path.Combine(Structs.dataDir, "UserData.json")))
                FirstTimeSetup();
            else
                LoadBasicAudio();

            playerTimer.Elapsed += new ElapsedEventHandler(playAudioTick);
            playerTimer.Interval = 500;
            playerTimer.Enabled = true;

        }

        private async void FirstTimeSetup()
        {
            //Some default Ultra Mega Funny sounds -_-
            var Sound1 = Utils.DownloadAudio("https://www.myinstants.com/media/sounds/fart-with-reverb.mp3", "Fard");
            var Sound2 = Utils.DownloadAudio("https://www.myinstants.com/media/sounds/yt1s_wU4BGgD.mp3", "WhatTheDogDoin");
            var Sound3 = Utils.DownloadAudio("https://www.myinstants.com/media/sounds/deeznuts_2.mp3", "DeezNuts");
            var Sound4 = Utils.DownloadAudio("https://www.myinstants.com/media/sounds/among-us-roundstart.mp3", "Amugus");
            var Sound5 = Utils.DownloadAudio("https://www.myinstants.com/media/sounds/vine-boom.mp3", "Vine Boom");

            await Task.WhenAll(Sound1, Sound2, Sound3, Sound4, Sound5);

            if (Sound1.Result != null && Sound2.Result != null && Sound3.Result != null && Sound4.Result != null && Sound5.Result != null) //Couldn't be bothered with making a statement for every one separately. I should have made an array of these but... I didnt 
            {
                availableSounds.Add(Sound1.Result);
                availableSounds.Add(Sound2.Result);
                availableSounds.Add(Sound3.Result);
                availableSounds.Add(Sound4.Result);
                availableSounds.Add(Sound5.Result);
            }

            foreach (var sound in availableSounds.ToList())
            {
                var soundControl = new Controls.SoundControl(sound);
                soundsPanel.Children.Add(soundControl);
            }

            userData.sounds = availableSounds;

            SaveUserData();
        }

        public void LoadBasicAudio()
        {
            availableSounds = userData.sounds;

            foreach (var sound in availableSounds.ToList())
            {
                if (File.Exists(sound.path))
                {
                    var soundControl = new Controls.SoundControl(sound);
                    soundsPanel.Children.Add(soundControl);
                }
                else
                {
                    userData.sounds.Remove(sound);
                    MessageBox.Show($"Linked sound '{sound.name}' located at: {sound.path} had to removed, because it was deleted from the disk");
                }
            }
            SaveUserData();
        }

        private void playAudioTick(object source, ElapsedEventArgs e)
        {
            if (rnd.NextDouble() > globalOdds) return;
            float _maxOdds = 0f;
            availableSounds.ForEach(x => { if (x.isEnabled) _maxOdds += x.odds; });
            float rngNum = (float)(rnd.NextDouble() * _maxOdds);
            float currentOdds = 0;
            foreach (Structs.Sound sound in availableSounds)
            {
                if (sound != null && sound.isEnabled)
                {
                    currentOdds += sound.odds;
                    if (currentOdds > rngNum)
                    {
                        var player = Utils.PlayAudio(sound.path, sound.volume * globalVolume);
                        if (player != null)
                        {
                            sound.mediaPlayers.Add(player);
                            return;
                        }
                        else
                        {
                            MessageBox.Show($"There was an error while playing the sound: {sound.name}");
                            return;
                        }
                    }
                }
            }
        }

        private void GlobalFrequencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            globalOdds = (float)Math.Pow(e.NewValue, 1.5d);
            if (currentGlobalFrequency != null)
            {
                currentGlobalFrequency.Content = $"{Math.Round(globalOdds * 100)}%";
                userData.globalFrequency = globalOdds;
                SaveUserData();
            }
        }

        private void GlobalVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            globalVolume = (float)e.NewValue;

            if (currentGlobalVolumeLabel != null)
            {
                currentGlobalVolumeLabel.Content = $"{Math.Round(globalVolume * 100)}%";
                userData.globalVolume = globalVolume;
                SaveUserData();
            }
        }

        private async void AddSoundFromLinkClicked(object sender, RoutedEventArgs e)
        {
            var link = linkTextBox.Text;
            var name = linkSoundNameBox.Text;

            if (userData.sounds.Any(sound => sound.name == name))
            {
                MessageBox.Show("Sound with this name already exists");
                return;
            }

            if (name == null || link == null)
            {
                MessageBox.Show("Please put in the name/link");
                return;
            }
            if (!Structs.AUDIO_FILE_EXTENSIONS.Contains(Path.GetExtension(link).ToLower()))
            {
                MessageBox.Show("Please check the link. Make sure it ends with e.g. '.mp3' or '.wav'");
                return;
            }

            var sound = await Utils.DownloadAudio(link, name);
            if (sound == null) return;
            userData.sounds.Add(sound);

            var soundControl = new Controls.SoundControl(sound);
            soundsPanel.Children.Add(soundControl);

            Utils.SaveUserData(userData);
        }

        public void SaveUserData()
        {
            userData.globalVolume = globalVolume;
            userData.globalFrequency = globalOdds;
            //userData.sounds.ToList().ForEach(sound => { if (sound.isDeleted) userData.sounds.Remove(sound); });
            Utils.SaveUserData(userData);
        }

        public async void RemoveSound(Controls.SoundControl soundControl)
        {
            if (soundControl == null || soundControl.DataContext == null) return;
            if (soundControl.DataContext is Structs.Sound soundData)
            {
                userData.sounds.Remove(soundData);

                soundData.isEnabled = false;
                soundsPanel.Children.Remove(soundControl);

                if (Path.GetDirectoryName(soundData.path) == Structs.dataDir)
                {
                    soundData.mediaPlayers.ForEach(p => p.Close());
                    await Task.Delay(10); //Better safe than sorry
                    File.Delete(soundData.path);
                }
            }

            //foreach (var child in soundsPanel.Children)
            //{
            //    var dataContext = (child as Controls.SoundControl).DataContext as Structs.Sound;
            //    if (DataContext != null && dataContext.path == soundToRemove.path) //NULL
            //        soundsPanel.Children.Remove(child as UIElement);
            //}
        }

        private void FileDropped(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //droppedSoundFiles.Clear();
                //fileTextBox.Text = "";
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file = files[0];

                if (!Structs.AUDIO_FILE_EXTENSIONS.Contains(Path.GetExtension(file).ToLower()))
                {
                    MessageBox.Show("Incorrect file extension. Make sure it ends with e.g. '.mp3' or '.wav'");
                    return;
                }

                var sound = new Structs.Sound() { path = file };
                droppedSoundFile = sound;

                fileTextBox.Text = Path.GetFileName(file);


                //Could use Foreach, but there is no good way to deal with naming all the files, and it's not that useful, so I'm gettng rid of it for now (for eternity);

                //int index = 0;
                //foreach(string file in files)
                //{
                //    index++;
                //    if (File.Exists(file))
                //    {
                //        var sound = new Structs.Sound() { path = file, name = name };
                //        droppedSoundFiles.Add(sound);

                //        fileTextBox.Text += Path.GetFileName(file);
                //        if(files[index+1] != null)
                //        {
                //            fileTextBox.Text += ", ";
                //        }
                //    }
                //}
            }
        }
        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void CopyDraggedFilesClicked(object sender, RoutedEventArgs e)
        {
            if (droppedSoundFile == null) return;
            if (userData.sounds.Any(sound => sound.name == droppedSoundFile.name))
            {
                MessageBox.Show("Sound with this name already exists");
                return;
            }
            var newSound = droppedSoundFile;
            if (fileSoundNameBox.Text == null || fileSoundNameBox.Text.Length <= 0)
            {
                MessageBox.Show("Please put in the name/link");
                return;
            }
            try
            {
                File.Copy(droppedSoundFile.path, Path.Combine(Structs.dataDir, Path.GetFileName(droppedSoundFile.path)));
            }
            catch (System.IO.IOException)
            {
                var dialogResult = MessageBox.Show("There was an error while copying the file (File with that same name might already exist. Do You want to override the current file?)", "File Error", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.OK)
                {
                    try
                    {
                        File.Copy(droppedSoundFile.path, Path.Combine(Structs.dataDir, Path.GetFileName(droppedSoundFile.path)), true);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("The issue persists, it might be caused by the file, its localization, or my lack of programming skills");
                        fileTextBox.Text = "Drop file here";
                        return;
                    }
                }
                else if (dialogResult == MessageBoxResult.No)
                {
                    return;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error while copying the file");
                fileTextBox.Text = "Drop file here";
                return;
            }
            newSound.path = Path.Combine(Structs.dataDir, Path.GetFileName(droppedSoundFile.path));
            newSound.name = fileSoundNameBox.Text;

            //availableSounds.Add(newSound);
            //availableSounds.Remove(soundToReplace);
            userData.sounds.Add(newSound);
            //userData.sounds.Remove(soundToReplace);

            var soundControl = new Controls.SoundControl(newSound);
            soundsPanel.Children.Add(soundControl);
            fileTextBox.Text = "Drop file here";
            //foreach (var child in soundsPanel.Children)
            //{
            //    var dataContext = (child as Controls.SoundControl).DataContext as Structs.Sound;
            //    if (DataContext != null && dataContext.path == droppedSoundFile.path)
            //        soundsPanel.Children.Remove(child as UIElement);
            //}
            //soundsPanel.Children.Cast<Controls.SoundControl>().ToList().ForEach(soundControl => soundControl.) //USE THE DATACONTEXT

            Utils.SaveUserData(userData);

            droppedSoundFile = null;

            //foreach(var sound in droppedSoundFiles)
            //{
            //    var newSound = sound;
            //    File.Copy(sound.path, Path.Combine(Structs.dataDir, Path.GetFileName(sound.path)));
            //    newSound.path = Path.Combine(Structs.dataDir, Path.GetFileName(sound.path));

            //    availableSounds.Add(newSound);
            //    userData.sounds.Add(newSound);

            //    var soundControl = new Controls.SoundControl(newSound);
            //    soundsPanel.Children.Add(soundControl);

            //    Utils.SaveUserData(userData);
            //}
        }

        private void LinkDraggedFilesClicked(object sender, RoutedEventArgs e)
        {
            if (droppedSoundFile == null) return;
            if (fileSoundNameBox.Text == null || fileSoundNameBox.Text.Length <= 0)
            {
                MessageBox.Show("Please put in the name/file");
                return;
            }
            if (userData.sounds.Any(sound => sound.name == droppedSoundFile.name))
            {
                MessageBox.Show("Sound with this name already exists");
                fileTextBox.Text = "Drop file here";
                return;
            }
            droppedSoundFile.name = fileSoundNameBox.Text;
            //availableSounds.Add(droppedSoundFile);
            userData.sounds.Add(droppedSoundFile);

            var soundControl = new Controls.SoundControl(droppedSoundFile);
            soundsPanel.Children.Add(soundControl);
            fileTextBox.Text = "Drop file here";

            Utils.SaveUserData(userData);

            //foreach (var sound in droppedSoundFiles)
            //{
            //    availableSounds.Add(sound);
            //    userData.sounds.Add(sound);

            //    var soundControl = new Controls.SoundControl(sound);
            //    soundsPanel.Children.Add(soundControl);

            //    Utils.SaveUserData(userData);
            //}

            droppedSoundFile = null;
        }
    }
}
