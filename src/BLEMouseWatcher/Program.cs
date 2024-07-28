using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace BLEMouseWatcher
{
    class Program
    {
        // "Magic" string for all BLE devices
        static readonly string allBleDevicesStr = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
        static readonly HashSet<string> monitoredDevices = new HashSet<string>
        {
            "RAPOO BleMouse"
        };

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

            int sleepMs = 5000;

            // Main loop
            while (true)
            {
                Thread.Sleep(sleepMs);

                if (watcher.Status == Windows.Devices.Enumeration.DeviceWatcherStatus.Stopped ||
                    watcher.Status == Windows.Devices.Enumeration.DeviceWatcherStatus.Created)
                {
                    if (IsMonitoredBluetoothLEDeviceConnected())
                    {
                        sleepMs = 10000;
                    }
                    else
                    {
                        watcher.Start();
                        sleepMs = 7000;
                    }

                    Thread.Sleep(sleepMs);
                }
            }
        }

        static bool IsMonitoredBluetoothLEDeviceConnected()
        {
            Task<DeviceInformationCollection> task = DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Connected)).AsTask();
            foreach (DeviceInformation device in task.Result)
            {
                if (monitoredDevices.Contains(device.Name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
