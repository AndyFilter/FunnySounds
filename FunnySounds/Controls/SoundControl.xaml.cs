using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FunnySounds.Controls
{
    /// <summary>
    /// Interaction logic for SoundControl.xaml
    /// </summary>
    public partial class SoundControl : UserControl
    {
        private Structs.Sound controledSound = null;
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
            controledSound.isEnabled = isActiveCheckBox.IsChecked!.Value;
            (App.Current.MainWindow as MainWindow)?.SaveUserData();
        }

        private void FrequencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controledSound.odds = (float)oddsSlider.Value;
            (App.Current.MainWindow as MainWindow)?.SaveUserData();
        }

        private void VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controledSound.volume = (float)volumeSlider.Value;
            (App.Current.MainWindow as MainWindow)?.SaveUserData();
        }

        private void NameTextChanged(object sender, TextChangedEventArgs e)
        {
            if (controledSound != null)
            {
                controledSound.name = nameTextBox.Text;
                (App.Current.MainWindow as MainWindow)?.SaveUserData();
            }
        }
    }
}
