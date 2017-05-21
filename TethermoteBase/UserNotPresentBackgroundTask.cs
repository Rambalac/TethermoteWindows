using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;

namespace Azi.TethermoteBase
{
    public sealed class UserNotPresentBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            try
            {
                if (!AppSettings.DisableOnUserNotPresent)
                {
                    Debug.WriteLine("UserNotPresent disabled");
                    return;
                }

                try
                {
                    Debug.WriteLine("User not Present");
                    var state = await Bluetooth.SwitchTethering(false);
                    await Tile.UpdateTile(state);
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