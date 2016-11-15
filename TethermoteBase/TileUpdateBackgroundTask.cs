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
    sealed public class TileUpdateBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
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
            finally
            {
                deferral.Complete();
            }
        }
    }
}
