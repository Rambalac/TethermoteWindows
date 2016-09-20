using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace TethermoteWindows
{
    sealed public class UserPresentBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            if (!AppSettings.EnableOnUserPresent) return;

            var deferral = taskInstance.GetDeferral();

            await App.SwitchTethering(true);

            deferral.Complete();
        }
    }

    sealed public class UserNotPresentBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            if (!AppSettings.DisableOnUserNotPresent) return;

            var deferral = taskInstance.GetDeferral();

            await App.SwitchTethering(false);

            deferral.Complete();
        }
    }
}
