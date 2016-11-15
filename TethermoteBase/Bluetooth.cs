using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Networking.Sockets;

namespace Azi.TethermoteBase
{
    public static class Bluetooth
    {
        public static IAsyncOperation<TetheringState> SwitchTethering(bool v)
        {
            return AsyncInfo.Run(async (cancel) => await SendBluetooth(v ? TetheringState.Enabled : TetheringState.Disabled));
        }

        private static readonly Guid serviceUuid = new Guid("5dc6ece2-3e0d-4425-ac00-e444be6b56cb");

        public static IAsyncOperation<IEnumerable<DeviceInfo>> GetDevices()
        {
            return AsyncInfo.Run(async (cancel) =>
            {
                var selector = BluetoothDevice.GetDeviceSelectorFromPairingState(true);
                var devices = await DeviceInformation.FindAllAsync(selector);
                return devices.Select(d => new DeviceInfo { Name = d.Name, Device = d });
            });
        }

        public static IAsyncOperation<TetheringState> SendBluetooth(TetheringState state)
        {
            return AsyncInfo.Run(async (cancel) =>
            {
                if (AppSettings.RemoteDevice == null) return TetheringState.Error;
                var device = (await GetDevices()).SingleOrDefault(d => d.Name == AppSettings.RemoteDevice);
                if (device == null) return TetheringState.Error;
                var result = await SendBluetooth(device.Device, state);

                return result;
            });
        }

        public static IAsyncOperation<StreamSocket> ConnectDevice(DeviceInformation dev)
        {
            return AsyncInfo.Run(async (cancel) =>
            {
                var service = await RfcommDeviceService.FromIdAsync(dev.Id);
                var socket = new StreamSocket();
                await socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionWithAuthentication);
                return socket;
            });
        }

        private static IAsyncAction PingDevice(string id)
        {
            return AsyncInfo.Run(async (cancel) =>
            {
                var bl = await BluetoothDevice.FromIdAsync(id);
                var service = (await bl.GetRfcommServicesAsync()).Services.FirstOrDefault();
                if (service == null) return;

                using (var socket = new StreamSocket())
                {
                    await socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionWithAuthentication);
                }
            });
        }

        public static IAsyncOperation<TetheringState> SendBluetooth(DeviceInformation dev, TetheringState state)
        {
            return AsyncInfo.Run(async (cancel) =>
            {
                for (var tryout = 10; tryout > 0; tryout--)
                {
                    try
                    {
                        string selector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.FromUuid(serviceUuid));
                        var devices = await DeviceInformation.FindAllAsync(selector);
                        var service = devices.SingleOrDefault(d => d.Id.StartsWith(dev.Id, StringComparison.OrdinalIgnoreCase));
                        if (service == null) throw new Exception("Tethermote Service not found");

                        using (var socket = await ConnectDevice(service))
                        {
                            using (var outstream = socket.OutputStream)
                            {
                                await outstream.WriteAsync(new byte[] { (byte)state }.AsBuffer());
                            }
                            using (var instream = socket.InputStream)
                            {
                                var buf = new Windows.Storage.Streams.Buffer(1);
                                var red = (await instream.ReadAsync(buf, 1, Windows.Storage.Streams.InputStreamOptions.Partial)).Length;

                                if (red == 1) return (TetheringState)buf.ToArray()[0];
                                Debug.WriteLine("No data");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        try
                        {
                            await PingDevice(dev.Id);
                        }
                        catch (Exception e2)
                        {
                            Debug.WriteLine(e2);
                        }
                        if (tryout != 1) await Task.Delay(100);
                    }
                }
                return TetheringState.Error;
            });
        }
    }
}
