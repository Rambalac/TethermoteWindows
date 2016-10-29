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
    sealed public class UserPresentBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            if (!AppSettings.EnableOnUserPresent)
            {
                Debug.WriteLine("UserPresent disabled");
                return;
            }

            try
            {
                await Bluetooth.SwitchTethering(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            deferral.Complete();
        }
    }

    sealed public class UserNotPresentBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            if (!AppSettings.DisableOnUserNotPresent)
            {
                Debug.WriteLine("UserNotPresent disabled");
                return;
            }

            try
            {
                await Bluetooth.SwitchTethering(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            deferral.Complete();
        }
    }
}
