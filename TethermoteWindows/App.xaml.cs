using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TethermoteWindows
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
#if DEBUG
            this.DebugSettings.EnableFrameRateCounter |= System.Diagnostics.Debugger.IsAttached;
#endif

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (!string.IsNullOrWhiteSpace(args.Arguments))
            {
                await TileClicked(args.Arguments == EnableSwitchArgument);
            }
            else
            if (args.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), args.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private async Task TileClicked(bool enable)
        {
            try
            {
                var state = await SwitchTethering(enable);
                await UpdateTile(state);
                if (state == TetheringStates.Enabled)
                {
                    await WaitForWiFiConnection();
                }
                else
                    if (state == TetheringStates.Error)
                {
                    await ShowError();
                }
            }
            catch (Exception)
            {
                await ShowError();
            }
            Exit();
        }

        static public async Task ShowError()
        {
            var dialog = new MessageDialog("Device can be too far for Bluetooth or its Bluetooth is disabled or device is turned off.\r\nIf device is near by but still does not work try to run Tethermote Settings on that device again.");
            await dialog.ShowAsync();
        }

        private async Task WaitForWiFiConnection()
        {
            var accessAllowed = await WiFiAdapter.RequestAccessAsync();
            if (accessAllowed == WiFiAccessStatus.Allowed)
            {
                var adapterList = await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                var wifiAdapter = await WiFiAdapter.FromIdAsync(adapterList[0].Id);

                for (int i = 0; i < 5; i++)
                {
                    await wifiAdapter.ScanAsync();
                    await Task.Delay(100);
                    if ((await wifiAdapter.NetworkAdapter.GetConnectedProfileAsync()) != null) break;
                }
            }
        }

        public static async Task<TetheringStates> SwitchTethering(bool v)
        {
            return await SendBluetooth(v ? TetheringStates.Enabled : TetheringStates.Disabled);
        }

        private static readonly Guid serviceUuid = new Guid("5dc6ece2-3e0d-4425-ac00-e444be6b56cb");

        public static async Task<IEnumerable<DeviceInfo>> GetDevices()
        {
            var selector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.FromUuid(serviceUuid));
            //var selector = BluetoothDevice.GetDeviceSelectorFromPairingState(true);
            var devices = await DeviceInformation.FindAllAsync(selector);
            return devices.Select(d => new DeviceInfo { Name = d.Name, Device = d });
        }

        public static async Task<TetheringStates> SendBluetooth(TetheringStates state)
        {
            if (AppSettings.RemoteDevice == null) return TetheringStates.Error;
            var device = (await GetDevices()).SingleOrDefault(d => d.Name == AppSettings.RemoteDevice);
            if (device == null) return TetheringStates.Error;
            var result = await SendBluetooth(device.Device, (byte)state);

            return (TetheringStates)result;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        const string EnableSwitchArgument = "enable";
        const string DisableSwitchArgument = "disable";

        public const string SwitchTileId = "SwitchTile";

        private static Rect GetElementRect(FrameworkElement _element)
        {
            var rectangleBounds = new Rect();
            rectangleBounds = _element.RenderTransform.TransformBounds(new Rect(0, 0, _element.Width, _element.Height));
            return rectangleBounds;
        }

        private static readonly Uri logoOn = new Uri("ms-appx:///Assets/widget_on.png");
        private static readonly Uri logoOff = new Uri("ms-appx:///Assets/widget_off.png");

        public static async Task AddSwitchTile(FrameworkElement sender, bool enabled)
        {
            var logo = enabled ? logoOn : logoOff;

            var s = new SecondaryTile(SwitchTileId,
                                                                "Title text shown on the tile",
                                                                "Name of the tile the user sees when searching for the tile",
                                                                (!enabled) ? EnableSwitchArgument : DisableSwitchArgument,
                                                                TileOptions.None,
                                                                logo);

            // Specify a foreground text value.
            s.ForegroundText = ForegroundText.Dark;
            await s.RequestCreateForSelectionAsync(GetElementRect(sender), Windows.UI.Popups.Placement.Below);
        }

        private async Task UpdateTile(TetheringStates state)
        {
            var enabled = state == TetheringStates.Enabled;
            var logo = enabled ? logoOn : logoOff;

            var s = new SecondaryTile(SwitchTileId,
                                                                "Title text shown on the tile",
                                                                "Name of the tile the user sees when searching for the tile",
                                                                (!enabled) ? EnableSwitchArgument : DisableSwitchArgument,
                                                                TileOptions.None,
                                                                logo);
            // Specify a foreground text value.
            s.ForegroundText = ForegroundText.Dark;
            await s.UpdateAsync();
        }

        public static async Task<byte> SendBluetooth(DeviceInformation dev, byte state)
        {
            var service = await RfcommDeviceService.FromIdAsync(dev.Id);
            var socket = new StreamSocket();
            await socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionWithAuthentication);
            using (var outstream = socket.OutputStream.AsStreamForWrite())
            {
                await outstream.WriteAsync(new byte[] { state }, 0, 1);
            }
            using (var instream = socket.InputStream.AsStreamForRead())
            {
                var buf = new byte[1];
                int red = await instream.ReadAsync(buf, 0, 1);
                if (red == 1) return buf[0];
                return 2;
            }

        }
    }
}
