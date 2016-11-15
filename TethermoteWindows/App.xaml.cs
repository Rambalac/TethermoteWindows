using Azi.TethermoteBase;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Azi.TethermoteWindows
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
            await RegisterBackgroundTasks();

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

        private async Task RegisterBackgroundTasks()
        {
            await BackgroundExecutionManager.RequestAccessAsync();
            RegisterBackgroundTask<UserPresentBackgroundTask>("UserPresent", new SystemTrigger(SystemTriggerType.UserPresent, false));
            RegisterBackgroundTask<UserPresentBackgroundTask>("SessionUserPresent", new SystemTrigger(SystemTriggerType.SessionConnected, false));
            RegisterBackgroundTask<UserNotPresentBackgroundTask>("UserNotPresent", new SystemTrigger(SystemTriggerType.UserAway, false));
            RegisterBackgroundTask<TileUpdateBackgroundTask>("TileUpdate", new TimeTrigger(30, false));
            RegisterBackgroundTask<TileUpdateBackgroundTask>("NetworkTileUpdate", new SystemTrigger(SystemTriggerType.NetworkStateChange, false));
        }

        private void RegisterBackgroundTask<T>(string taskName, IBackgroundTrigger trigger)
        {
            // check if task is already registered
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
                if (cur.Value.Name == taskName)
                {
                    return;
                }

            // register a new task
            var taskBuilder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskEntryPoint = typeof(T).FullName,
            };
            taskBuilder.SetTrigger(trigger);
            var myFirstTask = taskBuilder.Register();
        }
        private const string EnableSwitchArgument = "enable";
        private const string DisableSwitchArgument = "disable";

        private async Task TileClicked(bool enable)
        {
            try
            {
                var state = await Bluetooth.SwitchTethering(enable);
                await Tile.UpdateTile(state);
                if (state == TetheringState.Enabled)
                {
                    await WiFi.WaitForWiFiConnection();
                }
                else
                    if (state == TetheringState.Error)
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
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var dialog = new MessageDialog(loader.GetString("Message_BluetoothError"));
            await dialog.ShowAsync();
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
    }
}
