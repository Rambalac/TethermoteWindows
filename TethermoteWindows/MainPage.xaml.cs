namespace Azi.TethermoteWindows
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using TethermoteBase;
    using Windows.Devices.Radios;
    using Windows.System;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool connected = false;
        private DispatcherTimer timer;

        public MainPage()
        {
            InitializeComponent();
        }

        public static async Task ShowManual()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var dialog = new MessageDialog(loader.GetString("Message_Manual"));
            await dialog.ShowAsync();
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void AddSwitchTileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var enabled = await Bluetooth.SendBluetooth(TetheringState.GetState) == TetheringState.Enabled;
                await Tile.AddSwitchTile((FrameworkElement)sender, enabled);
            }
            catch (Exception)
            {
                await Tile.AddSwitchTile((FrameworkElement)sender, false);
            }
        }

        private async void ComboBox_DropDownClosed(object sender, object e)
        {
            if (DevicesComboBox.SelectedItem == null)
            {
                await ShowManual();
            }
        }

        private async void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            DevicesComboBox?.Items?.Clear();
            await RefreshDevices();
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };

            timer.Tick += Timer_Tick;
            timer.Start();

            if (DevicesComboBox.SelectedItem == null)
            {
                await ShowManual();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.RemoteDevice = ((DeviceInfo)DevicesComboBox.SelectedItem)?.Name;
        }

        private void Feedback_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void OpenDevicesSettings_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:bluetooth"));
        }

        private async void OpenWifiSettings_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:network-wifi"));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = await Radio.RequestAccessAsync();

                var newstate = await Bluetooth.SendBluetooth(TetheringState.GetState);
                connected = newstate == TetheringState.Enabled;
            }
            catch (Exception)
            {
            }

            UpdateButton();
        }

        private async Task RefreshDevices()
        {
            var devices = (await Bluetooth.GetDevices()).OrderBy(s => s.Name).ToList();
            var items = DevicesComboBox.Items.ToList().OfType<DeviceInfo>().OrderBy(s => s.Name).ToList();
            if (items.Count == devices.Count && items.SequenceEqual(devices))
            {
                return;
            }

            DevicesComboBox.Items?.Clear();
            foreach (var item in devices)
            {
                DevicesComboBox.Items?.Add(item);
                if (item.Name == AppSettings.RemoteDevice)
                {
                    DevicesComboBox.SelectedItem = item;
                }
            }
        }

        private async void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newstate = await App.SwitchTethering(!connected);

                connected = newstate == TetheringState.Enabled;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            UpdateButton();
        }

        private async void Timer_Tick(object sender, object e)
        {
            await RefreshDevices();
        }

        private void UpdateButton()
        {
            SwitchButton.Content = connected ? "Tap to Disconnect" : "Tap to Connect";
        }

        private void MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            DonationsButton.Flyout?.Hide();
        }
    }
}