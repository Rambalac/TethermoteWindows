using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Foundation;

namespace Azi.TethermoteBase
{
    public static class WiFi
    {
        public static IAsyncAction WaitForWiFiConnection()
        {
            return AsyncInfo.Run(async (cancel) =>
            {
                var accessAllowed = await WiFiAdapter.RequestAccessAsync();
                if (accessAllowed == WiFiAccessStatus.Allowed)
                {
                    var adapterList = await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                    var wifiAdapter = await WiFiAdapter.FromIdAsync(adapterList[0].Id);

                    for (int i = 0; i < 5; i++)
                    {
                        cancel.ThrowIfCancellationRequested();
                        await wifiAdapter.ScanAsync();
                        await Task.Delay(100);
                        if ((await wifiAdapter.NetworkAdapter.GetConnectedProfileAsync()) != null) break;
                    }
                }
            });
        }
    }
}
