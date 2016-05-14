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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
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

            if (!string.IsNullOrWhiteSpace(args.Arguments))
            {
                SwitchTethering(args.Arguments == EnableSwitchArgument);

                Exit();
            }
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

            if (await BackgroundExecutionManager.RequestAccessAsync() == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity)
            {
                AddSystemTrigger<UserPresentTask>(SystemTriggerType.UserPresent);
                AddSystemTrigger<UserAwayTask>(SystemTriggerType.UserAway);
            }
        }

        private static async Task SwitchTethering(bool v)
        {
            await SendBluetooth(v?)
            
        }

        private static void AddSystemTrigger<T>(SystemTriggerType trigger, string name = null) where T : IBackgroundTask
        {
            var systemTrigger = new SystemTrigger(trigger, false);
            var task = new BackgroundTaskBuilder();
            task.Name = name ?? typeof(T).FullName;
            task.TaskEntryPoint = typeof(T).FullName;
            task.SetTrigger(systemTrigger);

            if (!BackgroundTaskRegistration.AllTasks.Values.Any(t => t.Name == task.Name))
            {
                var taskRegistration = task.Register();
            }
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
            Rect rectangleBounds = new Rect();
            rectangleBounds = _element.RenderTransform.TransformBounds(new Rect(0, 0, _element.Width, _element.Height));
            return rectangleBounds;
        }

        public static async Task AddSwitchTile(FrameworkElement sender, bool enable)
        {
            var logo = new Uri("ms-appx:///Assets/s.png");
            var smallLogo = new Uri("ms-appx:///Assets/smallTile-sdk.png");

            var s = new SecondaryTile(SwitchTileId,
                                                                "Title text shown on the tile",
                                                                "Name of the tile the user sees when searching for the tile",
                                                                (enable) ? EnableSwitchArgument : DisableSwitchArgument,
                                                                TileOptions.ShowNameOnLogo,
                                                                logo);
            s.DisplayName = "mytiel";
            // Specify a foreground text value.
            s.ForegroundText = ForegroundText.Dark;
            await s.RequestCreateForSelectionAsync(GetElementRect(sender), Windows.UI.Popups.Placement.Below);
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
