using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;

namespace Azi.TethermoteBase
{
    sealed public class UserPresentBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            try
            {
                if (!AppSettings.EnableOnUserPresent)
                {
                    Debug.WriteLine("UserPresent disabled");
                    try
                    {
                        var state = await Bluetooth.SendBluetooth(TetheringState.GetState);
                        await Tile.UpdateTile(state);

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        try
                        {
                            await Tile.UpdateTile(TetheringState.Disabled);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }
                    return;
                }

                try
                {
                    Debug.WriteLine("User not Present");
                    ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
                    var con=connections.GetNetworkConnectivityLevel();
                    if (con == NetworkConnectivityLevel.InternetAccess||con==NetworkConnectivityLevel.ConstrainedInternetAccess) return;

                    var state = await Bluetooth.SwitchTethering(true);
                    await Tile.UpdateTile(state);
                    await WiFi.WaitForWiFiConnection();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
