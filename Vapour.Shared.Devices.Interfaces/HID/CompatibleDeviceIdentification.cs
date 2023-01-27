﻿using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID;

/// <summary>
///     Possible feature bit-flags.
/// </summary>
[Flags]
public enum CompatibleHidDeviceFeatureSet : ushort
{
    /// <summary>
    ///     Standard DS4 compatible communication (as it has been in DS4Win app for years)
    /// </summary>
    Default = 0,

    /// <summary>
    ///     The incoming HID report data structure does NOT send 0x11 packet even in DS4 mode over BT connection. If this flag
    ///     is set then accept "PC-friendly" 0x01 HID report data in BT just like how DS4 behaves in USB mode.
    /// </summary>
    OnlyInputData0x01 = 1 << 0,

    /// <summary>
    ///     Outgoing HID report write data structure does NOT support DS4 BT 0x11 data structure. Use only "USB type of" 0x05
    ///     data packets even in BT connections.
    /// </summary>
    OnlyOutputData0x05 = 1 << 2,

    /// <summary>
    ///     Gamepad doesn't support lightbar and rumble data writing at all. DS4Win app does not try to write out anything to
    ///     gamepad.
    /// </summary>
    NoOutputData = 1 << 3,

    /// <summary>
    ///     Gamepad doesn't send battery readings in the same format than DS4 gamepad (DS4Win app reports always 0% and starts
    ///     to blink lightbar). Skip reading a battery fields and report fixed 99% battery level to avoid "low battery" LED
    ///     flashes.
    /// </summary>
    NoBatteryReading = 1 << 4,

    /// <summary>
    ///     Gamepad doesn't support or need gyro calibration routines. Skip gyro calibration if this flag is set. Some gamepad
    ///     do have gyro, but don't support calibration or gyro sensors are missing.
    /// </summary>
    NoGyroCalib = 1 << 5,

    /// <summary>
    ///     Attempt to read volume levels for Gamepad headphone jack sink in Windows. Only with USB or Sony Wireless Adapter
    ///     connections.
    /// </summary>
    MonitorAudio = 1 << 6,

    /// <summary>
    ///     Accept the gamepad VID/PID even when it would be shown as vendor defined HID device on Windows (fex DS3 over
    ///     DsMiniHid gamepad may have vendor defined HID type)
    /// </summary>
    VendorDefinedDevice = 1 << 7
}
