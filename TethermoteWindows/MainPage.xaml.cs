using Azi.TethermoteBase;
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
using Windows.System;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Azi.TethermoteWindows
{
    

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox.Items.Clear();
            await RefreshDevices();
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };

            timer.Tick += Timer_Tick; ;
            timer.Start();
        }

        private async void Timer_Tick(object sender, object e)
        {
            await RefreshDevices();
        }

        private async Task RefreshDevices()
        {
            var devices = (await Bluetooth.GetDevices()).OrderBy(s => s.Name).ToList();
            var items = comboBox.Items.ToList().OfType<DeviceInfo>().OrderBy(s => s.Name).ToList();
            if (items.Count == devices.Count && items.SequenceEqual(devices)) return;
            comboBox.Items.Clear();
            foreach (var item in devices)
            {
                comboBox.Items.Add(item);
                if (item.Name == AppSettings.RemoteDevice) comboBox.SelectedItem = item;
            }
        }

        bool connected = false;

        private async void switchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newstate = await Bluetooth.SwitchTethering(!(connected));
                connected = newstate == TetheringState.Enabled;
            }
            catch (Exception)
            {
            }
            UpdateButton();
        }

        private void UpdateButton()
        {
            button.Content = (connected) ? "Tap to Disconnect" : "Tap to Connect";
        }

        private bool SwitchTileButtonEnabled => !SecondaryTile.Exists(App.SwitchTileId);

        private async void AddSwitchTileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var enabled = await Bluetooth.SendBluetooth(TetheringState.GetState) == TetheringState.Enabled;
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

        private async void openDevicesSettings_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:bluetooth"));
        }

        private async void openWifiSettings_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:network-wifi"));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var newstate = await Bluetooth.SendBluetooth(TetheringState.GetState);
                connected = newstate == TetheringState.Enabled;
            }
            catch (Exception)
            {
            }
            UpdateButton();
        }
    }
}
