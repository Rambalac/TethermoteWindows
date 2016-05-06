using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TethermoteWindows
{
    public class DeviceInfo
    {
        public string Name { get; set; }
        public DeviceInformation Device { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly Guid serviceUuid = new Guid("5dc6ece2-3e0d-4425-ac00-e444be6b56cb");
        public async Task<IList<DeviceInfo>> GetDevices()
        {
            try
            {
                var selector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.FromUuid(serviceUuid));
                //var selector = BluetoothDevice.GetDeviceSelectorFromPairingState(true);
                var devices = await DeviceInformation.FindAllAsync(selector);
                return devices.Select(d => new DeviceInfo { Name = d.Name, Device = d }).ToList();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async Task<byte> sendBluetooth(DeviceInformation dev, byte state)
        {
            var service = await RfcommDeviceService.FromIdAsync(dev.Id);
            var socket = new StreamSocket();
            await socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionWithAuthentication);
            using (var outstream = socket.OutputStream.AsStreamForWrite())
            {
                await outstream.WriteAsync(new byte[] { state }, 0, 1);
            }
            using (var instream = socket.InputStream.AsStreamForRead())
            {
                var buf = new byte[1];
                int red = await instream.ReadAsync(buf, 0, 1);
                if (red == 1) return buf[0];
                return 2;
            }

        }

        private async void comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox.Items.Clear();
            foreach (var item in await GetDevices())
            {
                comboBox.Items.Add(item);
            }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var device = (DeviceInfo)comboBox.SelectedItem;
            if (device == null) return;
            var newstate = await sendBluetooth(device.Device,
                (button.IsChecked ?? false) ? (byte)1 : (byte)0);
            if (newstate > 1) return;
            button.IsChecked = (newstate != 0);
        }
    }
}
