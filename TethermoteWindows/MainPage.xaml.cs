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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TethermoteWindows
{
    public class DeviceInfo
    {
        public string Name { get; set; }
        public DeviceInformation Device { get; set; }
        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Name == ((DeviceInfo)obj).Name;;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

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
            var devices = (await App.GetDevices()).OrderBy(s => s.Name).ToList();
            if (comboBox.Items.Count == devices.Count&& comboBox.Items.ToList().OfType<DeviceInfo>().OrderBy(s=>s.Name).SequenceEqual(devices)) return;
            comboBox.Items.Clear();
            foreach (var item in devices)
            {
                comboBox.Items.Add(item);
                if (item.Name == AppSettings.RemoteDevice) comboBox.SelectedItem = item;
            }
        }

        private async void switchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newstate = await App.SwitchTethering(!(button.IsChecked ?? false));
                button.IsChecked = newstate == TetheringState.Enabled;
            }
            catch (Exception)
            {
                button.IsChecked = false;
            }
        }

        private bool SwitchTileButtonEnabled => !SecondaryTile.Exists(App.SwitchTileId);

        private async void AddSwitchTileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var enabled = await App.SendBluetooth(TetheringState.GetState) == TetheringState.Enabled;
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
    }
}
