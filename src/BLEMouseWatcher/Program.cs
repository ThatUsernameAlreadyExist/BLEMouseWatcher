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
            ManualResetEvent watcherStopEvent = new ManualResetEvent(false);
            ManualResetEvent disconnectEvent  = new ManualResetEvent(false);

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
                watcherStopEvent.Set();
            };

            while (true)
            {
                BluetoothLEDevice bluetoothLEDevice = findMonitoredBluetoothLEDevice();
                if (bluetoothLEDevice == null)
                {
                    Thread.Sleep(3000);
                }
                else
                {
                    if (bluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
                    {
                        disconnectEvent.Set();
                    }

                    bluetoothLEDevice.ConnectionStatusChanged += (BluetoothLEDevice sender, object arg) =>
                    {
                        if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
                        {
                            disconnectEvent.Set();
                        }
                        else
                        {
                            disconnectEvent.Reset();
                            if (watcher.Status == Windows.Devices.Enumeration.DeviceWatcherStatus.Started)
                            {
                                watcher.Stop();
                            }
                        }
                    };

                    while (bluetoothLEDevice != null)
                    {
                        if (disconnectEvent.WaitOne(3000))
                        {
                            if (watcher.Status == Windows.Devices.Enumeration.DeviceWatcherStatus.Stopped ||
                                watcher.Status == Windows.Devices.Enumeration.DeviceWatcherStatus.Created)
                            {
                                watcherStopEvent.Reset();
                                watcher.Start();
                            }

                            watcherStopEvent.WaitOne();
                        }
                        else if (bluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
                        {
                            bluetoothLEDevice.Dispose();
                            bluetoothLEDevice = null;
                        }

                        Thread.Sleep(500);
                    }
                }
            }
        }

        static BluetoothLEDevice findMonitoredBluetoothLEDevice()
        {
            try
            {
                Task<DeviceInformationCollection> task = DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelector()).AsTask();
                foreach (DeviceInformation device in task.Result)
                {
                    if (monitoredDevices.Contains(device.Name))
                    {
                        Task<BluetoothLEDevice> bleDevice = BluetoothLEDevice.FromIdAsync(device.Id).AsTask();
                        try
                        {
                            return bleDevice.Result;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}
