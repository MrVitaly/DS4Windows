﻿using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4Windows.Shared.Devices.HID;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllerItemViewModel : ViewModel<IControllerItemViewModel>, IControllerItemViewModel
    {
        private const string imageLocationRoot = "pack://application:,,,/DS4Windows.Client.Modules;component/Controllers/Images";
        private IProfilesService profilesService;
        private static BitmapImage dualSenseImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/dualsense.jpg", UriKind.Absolute));
        private static BitmapImage dualShockV2ImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/dualshockv2.jpg", UriKind.Absolute));
        private static BitmapImage joyconLeftImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/joyconleft.jpg", UriKind.Absolute));
        private static BitmapImage joyconRightImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/joyconright.jpg", UriKind.Absolute));
        private static BitmapImage switchProImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/switchpro.jpg", UriKind.Absolute));
        private static BitmapImage BluetoothImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/BT.png", UriKind.Absolute));
        private static BitmapImage UsbImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/USB_white.png", UriKind.Absolute));

        public ControllerItemViewModel(IProfilesService profilesService)
        {
            this.profilesService = profilesService;
        }

        #region Props

        private ICompatibleHidDevice? device;



        private PhysicalAddress serial;
        public PhysicalAddress Serial
        {
            get => serial;
            private set => SetProperty(ref serial, value);
        }


        private BitmapImage deviceImage;
        public BitmapImage DeviceImage
        {
            get => deviceImage; 
            private set => SetProperty(ref deviceImage, value); 
        }

        private string displayText;
        public string DisplayText
        {
            get => displayText; 
            private set => SetProperty(ref displayText, value); 
        }

        private BitmapImage connectionTypeImage;
        public BitmapImage ConnectionTypeImage
        {
            get => connectionTypeImage; 
            private set => SetProperty(ref connectionTypeImage, value); 
        }

        private decimal batteryPercentage;
        public decimal BatteryPercentage
        {
            get => batteryPercentage; 
            private set => SetProperty(ref batteryPercentage, value); 
        }

        private Guid selectedProfileId;
        public Guid SelectedProfileId
        {
            get => selectedProfileId; 
            set => SetProperty(ref selectedProfileId, value); 
        }

        private SolidColorBrush currentColor;
        public SolidColorBrush CurrentColor
        {
            get => currentColor;
            set => SetProperty(ref currentColor, value);
        } 

        #endregion

        public void SetDevice(ICompatibleHidDevice device)
        {
            this.device = device;
            MapProperties();
        }

        private void MapProperties()
        {
            DisplayText = $"{device.DeviceType} ({device.SerialNumberString})";
            Serial = device.Serial;

            if (device.Connection == ConnectionType.Bluetooth)
            {
                ConnectionTypeImage = BluetoothImageLocation;
            }
            else
            {
                ConnectionTypeImage = UsbImageLocation;
            }

            switch (device.DeviceType)
            {
                case InputDeviceType.DualSense: 
                    DeviceImage = dualSenseImageLocation;
                    break;
                case InputDeviceType.DualShock4: 
                    DeviceImage = dualShockV2ImageLocation;
                    break;
                case InputDeviceType.JoyConL: 
                    DeviceImage = joyconLeftImageLocation;
                    break;
                case InputDeviceType.JoyConR: 
                    DeviceImage = joyconRightImageLocation;
                    break;
                case InputDeviceType.SwitchPro: 
                    DeviceImage = switchProImageLocation;
                    break;
            }

            SelectedProfileId = profilesService.ActiveProfiles.Single(p => p.DeviceId != null && p.DeviceId.Equals(device.Serial)).Id;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                device = null;
                profilesService = null;
            }

            base.Dispose(disposing);
        }
    }
}
