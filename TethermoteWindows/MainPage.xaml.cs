using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TethermoteWindows
{
    public class DeviceInfo
    {
        public string Name { get; set; }
        public DeviceInformation Device { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox.Items.Clear();
            var devices = await App.GetDevices();
            foreach (var item in devices)
            {
                comboBox.Items.Add(item);
                if (item.Name == AppSettings.RemoteDevice) comboBox.SelectedItem = item;
            }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var newstate = await App.SwitchTethering(!(button.IsChecked ?? false));
            if (newstate != TetheringStates.Enabled || newstate != TetheringStates.Disabled) return;
            button.IsChecked = (newstate != 0);
        }

        private bool SwitchTileButtonEnabled => !SecondaryTile.Exists(App.SwitchTileId);

        private async void AddSwitchTileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var enabled = await App.SendBluetooth(TetheringStates.GetState) == TetheringStates.Enabled;
                await App.AddSwitchTile((FrameworkElement)sender, enabled);
            }
            catch (Exception)
            {
                await App.AddSwitchTile((FrameworkElement)sender, false);
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.RemoteDevice = ((DeviceInfo)comboBox.SelectedItem)?.Name;
        }
    }
}
