using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Popups;

namespace Azi.TethermoteBase
{
    sealed public class UserNotPresentBackgroundTask : IBackgroundTask
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