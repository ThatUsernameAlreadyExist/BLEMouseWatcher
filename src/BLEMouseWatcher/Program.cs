using Windows.Devices.Enumeration;
using System.Threading;

namespace BLEMouseWatcher
{
    class Program
    {
        // "Magic" string for all BLE devices
        static string allBleDevicesStr = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
        static string[] requestedBLEProperties = {  };

        static void Main(string[] args)
        {
            // Start endless BLE device watcher
            var watcher = DeviceInformation.CreateWatcher(allBleDevicesStr, requestedBLEProperties, DeviceInformationKind.AssociationEndpoint);
            watcher.Added += (DeviceWatcher sender, DeviceInformation devInfo) =>
            {
            };
            watcher.Updated += (DeviceWatcher sender, DeviceInformationUpdate devInfo) =>
            {
            };
            watcher.EnumerationCompleted += (DeviceWatcher sender, object arg) =>
            {
                sender.Stop();
            };
            watcher.Stopped += (DeviceWatcher sender, object arg) =>
            {
            };

            // Main loop
            while (true)
            {
                Thread.Sleep(5000);

                if (watcher.Status == Windows.Devices.Enumeration.DeviceWatcherStatus.Stopped ||
                    watcher.Status == Windows.Devices.Enumeration.DeviceWatcherStatus.Created)
                {
                    watcher.Start();
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
