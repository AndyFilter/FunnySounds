using System.Windows;
using System.Windows.Controls;

namespace FunnySounds.Controls
{
    /// <summary>
    /// Interaction logic for SoundControl.xaml
    /// </summary>
    public partial class SoundControl : UserControl
    {
        private Structs.Sound? controledSound = null;
        private MainWindow mainWindow = (App.Current.MainWindow as MainWindow)!;
        public SoundControl(Structs.Sound sound)
        {
            InitializeComponent();

            this.DataContext = sound;

            if (sound != null)
            {
                controledSound = sound;

                nameTextBox.Text = sound.name;
                isActiveCheckBox.IsChecked = sound.isEnabled;
                oddsSlider.Value = sound.odds;
                volumeSlider.Value = sound.volume;
            }
        }

        private void ActiveCB_Clicked(object sender, RoutedEventArgs e)
        {
            if (controledSound != null)
            {
                controledSound.isEnabled = isActiveCheckBox.IsChecked!.Value;
                mainWindow.SaveUserData();
            }
        }

        private void FrequencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (controledSound != null)
            {
                controledSound.odds = (float)oddsSlider.Value;
                mainWindow.SaveUserData();
            }
        }

        private void VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (controledSound != null)
            {
                controledSound.volume = (float)volumeSlider.Value;
                mainWindow.SaveUserData();
            }
        }

        private void NameTextChanged(object sender, TextChangedEventArgs e)
        {
            if (controledSound != null)
            {
                controledSound.name = nameTextBox.Text;
                mainWindow.SaveUserData();
            }
        }

        private void DeleteSoundClicked(object sender, RoutedEventArgs e)
        {
            if (controledSound != null)
            {
                mainWindow.RemoveSound(this);
                mainWindow.SaveUserData();
            }
        }
    }
}
