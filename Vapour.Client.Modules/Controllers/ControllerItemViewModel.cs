﻿using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using AutoMapper;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Server.Controller;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.Controllers;

public sealed class ControllerItemViewModel :
    ViewModel<IControllerItemViewModel>,
    IControllerItemViewModel
{
    private const string ImageLocationRoot =
        "pack://application:,,,/Vapour.Client.Modules;component/Controllers/Images";

    public static BitmapImage GetDeviceImage(string name)
    {
        try
        {
            var uri = new Uri($"{ImageLocationRoot}/{name.ToLower()}.jpg", UriKind.Absolute);
            var image = new BitmapImage(uri);
            return image;
        }
        catch
        {
            return null;
        }
    }

    public static BitmapImage BluetoothImageLocation = new(new Uri($"{ImageLocationRoot}/BT.png", UriKind.Absolute));
    public static BitmapImage UsbImageLocation = new(new Uri($"{ImageLocationRoot}/USB.png", UriKind.Absolute));
    private readonly IMapper _mapper;
    private readonly IControllerServiceClient _controllerServiceClient;

    public ControllerItemViewModel(IMapper mapper, IControllerServiceClient controllerServiceClient)
    {
        _mapper = mapper;
        _controllerServiceClient = controllerServiceClient;
    }

    public void SetDevice(ControllerConnectedMessage device)
    {
        ConfigurationSetFromUser = false;
        _mapper.Map(device, this);
        ConfigurationSetFromUser = true;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedProfileId) || 
            e.PropertyName == nameof(IsPassthru) ||
            e.PropertyName == nameof(OutputDeviceType))
        {
            if (ConfigurationSetFromUser)
            {
                if (!CurrentConfiguration.IsGameConfiguration)
                {
                    _controllerServiceClient.SaveDefaultControllerConfiguration(Serial, CurrentConfiguration);
                }
                else
                {
                    _controllerServiceClient.SaveGameConfiguration(Serial, CurrentConfiguration.GameInfo, CurrentConfiguration);
                }
            }
        }

        base.OnPropertyChanged(e);
    }

    #region Props
    
    public bool ConfigurationSetFromUser { get; set; } = true;

    private string _serial;
    public string Serial
    {
        get => _serial;
        private set => SetProperty(ref _serial, value);
    }

    private BitmapImage _deviceImage;
    public BitmapImage DeviceImage
    {
        get => _deviceImage;
        private set => SetProperty(ref _deviceImage, value);
    }

    private string _displayText;
    public string DisplayText
    {
        get => _displayText;
        private set => SetProperty(ref _displayText, value);
    }

    private BitmapImage _connectionTypeImage;
    public BitmapImage ConnectionTypeImage
    {
        get => _connectionTypeImage;
        private set => SetProperty(ref _connectionTypeImage, value);
    }

    private decimal _batteryPercentage;
    public decimal BatteryPercentage
    {
        get => _batteryPercentage;
        private set => SetProperty(ref _batteryPercentage, value);
    }

    public Guid SelectedProfileId
    {
        get => CurrentConfiguration.ProfileId;
        set
        {
            if (CurrentConfiguration.ProfileId != value)
            {
                CurrentConfiguration.ProfileId = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsProfileSetEnabled
    {
        get
        {
            return !IsPassthru && OutputDeviceType != OutputDeviceType.None;
        }
    }

    private SolidColorBrush _currentColor;
    public SolidColorBrush CurrentColor
    {
        get => _currentColor;
        set => SetProperty(ref _currentColor, value);
    }

    private bool _isFiltered;
    public bool IsFiltered
    {
        get => _isFiltered;
        set => SetProperty(ref _isFiltered, value);
    }

    public string InstanceId { get; set; }
    public string ParentInstance { get; set; }

    public bool IsPassthru
    {
        get => CurrentConfiguration.IsPassthru;
        set
        {
            if (CurrentConfiguration.IsPassthru != value)
            {
                CurrentConfiguration.IsPassthru = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProfileSetEnabled));
            }
        }
    }

    public OutputDeviceType OutputDeviceType
    {
        get => CurrentConfiguration.OutputDeviceType;
        set
        {
            if (CurrentConfiguration.OutputDeviceType != value)
            {
                CurrentConfiguration.OutputDeviceType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProfileSetEnabled));
            }
        }
    }

    public string OutputGroupName
    {
        get
        {
            return $"{Serial}_OutputGroup";
        }
    }

    public bool IsGameConfiguration
    {
        get
        {
            return CurrentConfiguration.IsGameConfiguration;
        }
    }

    public GameInfo GameInfo
    {
        get
        {
            return CurrentConfiguration.GameInfo;
        }
    }

    public string GameSource
    {
        get
        {
            return CurrentConfiguration.IsGameConfiguration ? CurrentConfiguration.GameInfo.GameSource == Shared.Devices.Services.Configuration.GameSource.Steam ? "steam" : "microsoft-xbox" : string.Empty;
        }
    }

    private ControllerConfiguration _currentConfiguration;
    public ControllerConfiguration CurrentConfiguration
    {
        get => _currentConfiguration;
        set
        {
            SetProperty(ref _currentConfiguration, value);
            OnPropertyChanged(nameof(SelectedProfileId));
            OnPropertyChanged(nameof(IsPassthru));
            OnPropertyChanged(nameof(OutputDeviceType));
            OnPropertyChanged(nameof(IsProfileSetEnabled));
            OnPropertyChanged(nameof(IsGameConfiguration));
            OnPropertyChanged(nameof(GameInfo));
            OnPropertyChanged(nameof(GameSource));
        }
    }

    #endregion
}