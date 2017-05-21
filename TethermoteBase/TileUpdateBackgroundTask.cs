using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;

namespace Azi.TethermoteBase
{
    public sealed class TileUpdateBackgroundTask : IBackgroundTask
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