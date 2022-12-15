﻿using System.Collections.ObjectModel;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

public interface IInputSourceService
{
    ReadOnlyObservableCollection<IInputSource> InputSources { get; }

    void ControllerArrived(ICompatibleHidDevice device);

    void ControllerDeparted(ICompatibleHidDevice device);
}