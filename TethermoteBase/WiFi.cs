using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;
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
                if (!await EnableWiFi())
                {
                    return;
                }

                var accessAllowed = await WiFiAdapter.RequestAccessAsync();
                if (accessAllowed == WiFiAccessStatus.Allowed)
                {
                    var adapterList = await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                    var wifiAdapter = await WiFiAdapter.FromIdAsync(adapterList[0].Id);

                    for (var i = 0; i < 5; i++)
                    {
                        cancel.ThrowIfCancellationRequested();
                        await wifiAdapter.ScanAsync();
                        await Task.Delay(100);
                        if (await wifiAdapter.NetworkAdapter.GetConnectedProfileAsync() != null) break;
                    }
                }
            });
        }

        private static async Task<bool> EnableWiFi()
        {
            var result = await Radio.RequestAccessAsync();
            if (result == RadioAccessStatus.Allowed)
            {
                var wifi = (await Radio.GetRadiosAsync()).FirstOrDefault(radio => radio.Kind == RadioKind.WiFi);
                if (wifi == null)
                {
                    return false;
                }

                if (wifi.State != RadioState.On)
                {
                    await wifi.SetStateAsync(RadioState.On);
                }

                return true;
            }

            return false;
        }
    }
}