﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DS4Windows;
using DS4Windows.InputDevices;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Logging;
using Microsoft.Win32;
using Color = System.Windows.Media.Color;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public partial class ProfileSettingsViewModel
    {
        private readonly IAppSettingsService appSettings;

        private readonly ControlService rootHub;
        public EventHandler DInputOnlyChanged;

        private string gyroControlsTrigDisplay = "Always On";

        private int gyroMouseSmoothMethodIndex;


        private int gyroMouseStickSmoothMethodIndex;

        private string gyroMouseStickTrigDisplay = "Always On";


        private string gyroMouseTrigDisplay = "Always On";

        private string gyroSwipeTrigDisplay = "Always On";

        private bool heavyRumbleActive;
        private readonly SolidColorBrush lightbarColBrush = new();

        private readonly ImageBrush lightbarImgBrush = new();

        private bool lightRumbleActive;

        private double mouseOffsetSpeed;

        private int outputMouseSpeed;

        private readonly int[] saSteeringRangeValues =
            new int[9] { 90, 180, 270, 360, 450, 720, 900, 1080, 1440 };

        private int tempControllerIndex;

        private string touchDisInvertString = "None";

        private readonly int[] touchpadInvertToValue = new int[4] { 0, 2, 1, 3 };

        private List<TriggerModeChoice> triggerModeChoices = new()
        {
            new TriggerModeChoice("Normal", TriggerMode.Normal)
        };

        public ProfileSettingsViewModel(IAppSettingsService appSettings, ControlService service, int device)
        {
            this.appSettings = appSettings;
            rootHub = service;
            Device = device;
            FuncDevNum = device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT ? device : 0;
            tempControllerIndex = ControllerTypeIndex;
            Global.OutDevTypeTemp[device] = OutContType.X360;
            TempBTPollRateIndex = ProfilesService.Instance.ActiveProfiles.ElementAt(device).BluetoothPollRate;

            outputMouseSpeed = CalculateOutputMouseSpeed(ButtonMouseSensitivity);
            mouseOffsetSpeed = RawButtonMouseOffset * outputMouseSpeed;

            /*ImageSourceConverter sourceConverter = new ImageSourceConverter();
            ImageSource temp = sourceConverter.
                ConvertFromString($"{Global.Instance.ASSEMBLY_RESOURCE_PREFIX}component/Resources/rainbowCCrop.png") as ImageSource;
            lightbarImgBrush.ImageSource = temp.Clone();
            */
            var tempResourceUri = new Uri($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/rainbowCCrop.png");
            var tempBitmap = new BitmapImage();
            tempBitmap.BeginInit();
            // Needed for some systems not using the System default color profile
            tempBitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            tempBitmap.UriSource = tempResourceUri;
            tempBitmap.EndInit();
            lightbarImgBrush.ImageSource = tempBitmap.Clone();

            PresetMenuUtil = new PresetMenuHelper(device);
            gyroMouseSmoothMethodIndex = FindGyroMouseSmoothMethodIndex();
            gyroMouseStickSmoothMethodIndex = FindGyroMouseStickSmoothMethodIndex();

            SetupEvents();
        }
        
        public PresetMenuHelper PresetMenuUtil { get; }

        public event EventHandler LightbarModeIndexChanged;
        public event EventHandler LightbarBrushChanged;
        public event EventHandler MainColorChanged;
        public event EventHandler MainColorStringChanged;
        public event EventHandler MainColorRChanged;
        public event EventHandler MainColorRStringChanged;
        public event EventHandler MainColorGChanged;
        public event EventHandler MainColorGStringChanged;
        public event EventHandler MainColorBChanged;
        public event EventHandler MainColorBStringChanged;
        public event EventHandler LowColorChanged;
        public event EventHandler LowColorRChanged;
        public event EventHandler LowColorRStringChanged;
        public event EventHandler LowColorGChanged;
        public event EventHandler LowColorGStringChanged;
        public event EventHandler LowColorBChanged;
        public event EventHandler LowColorBStringChanged;

        public event EventHandler FlashColorChanged;

        public event EventHandler ChargingColorChanged;
        public event EventHandler ChargingColorVisibleChanged;
        public event EventHandler RainbowChanged;

        public event EventHandler RainbowExistsChanged;
        public event EventHandler HeavyRumbleActiveChanged;
        public event EventHandler LightRumbleActiveChanged;
        public event EventHandler ButtonMouseSensitivityChanged;
        public event EventHandler ButtonMouseVerticalScaleChanged;
        public event EventHandler ButtonMouseOffsetChanged;
        public event EventHandler OutputMouseSpeedChanged;
        public event EventHandler MouseOffsetSpeedChanged;
        public event EventHandler LaunchProgramExistsChanged;
        public event EventHandler LaunchProgramChanged;
        public event EventHandler LaunchProgramNameChanged;
        public event EventHandler LaunchProgramIconChanged;
        public event EventHandler IdleDisconnectExistsChanged;
        public event EventHandler IdleDisconnectChanged;
        public event EventHandler GyroOutModeIndexChanged;
        public event EventHandler SASteeringWheelEmulationAxisIndexChanged;
        public event EventHandler SASteeringWheelUseSmoothingChanged;
        public event EventHandler LSDeadZoneChanged;
        public event EventHandler RSDeadZoneChanged;
        public event EventHandler LSCustomCurveSelectedChanged;
        public event EventHandler RSCustomCurveSelectedChanged;
        public event EventHandler LSOutputIndexChanged;
        public event EventHandler RSOutputIndexChanged;
        public event EventHandler L2DeadZoneChanged;
        public event EventHandler R2DeadZoneChanged;
        public event EventHandler L2CustomCurveSelectedChanged;
        public event EventHandler R2CustomCurveSelectedChanged;
        public event EventHandler L2TriggerModeChanged;
        public event EventHandler R2TriggerModeChanged;
        public event EventHandler SXDeadZoneChanged;
        public event EventHandler SZDeadZoneChanged;
        public event EventHandler SXCustomCurveSelectedChanged;
        public event EventHandler SZCustomCurveSelectedChanged;
        public event EventHandler TouchpadOutputIndexChanged;
        public event EventHandler TouchSenExistsChanged;
        public event EventHandler TouchSensChanged;
        public event EventHandler TouchScrollExistsChanged;
        public event EventHandler TouchScrollChanged;
        public event EventHandler TouchTapExistsChanged;
        public event EventHandler TouchTapChanged;
        public event EventHandler GyroMouseSmoothChanged;
        public event EventHandler GyroMouseSmoothMethodIndexChanged;
        public event EventHandler GyroMouseWeightAvgPanelVisibilityChanged;
        public event EventHandler GyroMouseOneEuroPanelVisibilityChanged;
        public event EventHandler GyroMouseStickSmoothMethodIndexChanged;
        public event EventHandler GyroMouseStickWeightAvgPanelVisibilityChanged;
        public event EventHandler GyroMouseStickOneEuroPanelVisibilityChanged;
        public event EventHandler GyroMouseStickMaxOutputChanged;
        public event EventHandler TouchDisInvertStringChanged;
        public event EventHandler GyroControlsTrigDisplayChanged;
        public event EventHandler GyroMouseTrigDisplayChanged;

        public event EventHandler GyroMouseStickTrigDisplayChanged;
        public event EventHandler GyroSwipeTrigDisplayChanged;

        private int FindGyroMouseSmoothMethodIndex()
        {
            var result = 0;
            var tempInfo = Global.Instance.Config.GyroMouseInfo[Device];
            if (tempInfo.smoothingMethod == GyroMouseInfo.SmoothingMethod.OneEuro ||
                tempInfo.smoothingMethod == GyroMouseInfo.SmoothingMethod.None)
                result = 0;
            else if (tempInfo.smoothingMethod == GyroMouseInfo.SmoothingMethod.WeightedAverage) result = 1;

            return result;
        }

        private int FindGyroMouseStickSmoothMethodIndex()
        {
            var result = 0;
            var tempInfo = Global.Instance.Config.GyroMouseStickInfo[Device];
            switch (tempInfo.Smoothing)
            {
                case GyroMouseStickInfo.SmoothingMethod.OneEuro:
                case GyroMouseStickInfo.SmoothingMethod.None:
                    result = 0;
                    break;
                case GyroMouseStickInfo.SmoothingMethod.WeightedAverage:
                    result = 1;
                    break;
            }

            return result;
        }

        private void CalcProfileFlags(object sender, EventArgs e)
        {
            Global.Instance.Config.CacheProfileCustomsFlags(Device);
        }

        private void SetupEvents()
        {
            MainColorChanged += ProfileSettingsViewModel_MainColorChanged;
            MainColorRChanged += (sender, args) =>
            {
                MainColorRStringChanged?.Invoke(this, EventArgs.Empty);
                MainColorStringChanged?.Invoke(this, EventArgs.Empty);
                LightbarBrushChanged?.Invoke(this, EventArgs.Empty);
            };
            MainColorGChanged += (sender, args) =>
            {
                MainColorGStringChanged?.Invoke(this, EventArgs.Empty);
                MainColorStringChanged?.Invoke(this, EventArgs.Empty);
                LightbarBrushChanged?.Invoke(this, EventArgs.Empty);
            };
            MainColorBChanged += (sender, args) =>
            {
                MainColorBStringChanged?.Invoke(this, EventArgs.Empty);
                MainColorStringChanged?.Invoke(this, EventArgs.Empty);
                LightbarBrushChanged?.Invoke(this, EventArgs.Empty);
            };

            RainbowChanged += (sender, args) => { LightbarBrushChanged?.Invoke(this, EventArgs.Empty); };

            ButtonMouseSensitivityChanged += (sender, args) =>
            {
                OutputMouseSpeed = CalculateOutputMouseSpeed(ButtonMouseSensitivity);
                MouseOffsetSpeed = RawButtonMouseOffset * OutputMouseSpeed;
            };

            GyroOutModeIndexChanged += CalcProfileFlags;
            SASteeringWheelEmulationAxisIndexChanged += CalcProfileFlags;
            LSOutputIndexChanged += CalcProfileFlags;
            RSOutputIndexChanged += CalcProfileFlags;
            ButtonMouseOffsetChanged += ProfileSettingsViewModel_ButtonMouseOffsetChanged;
            GyroMouseSmoothMethodIndexChanged += ProfileSettingsViewModel_GyroMouseSmoothMethodIndexChanged;
            GyroMouseStickSmoothMethodIndexChanged += ProfileSettingsViewModel_GyroMouseStickSmoothMethodIndexChanged;
        }

        private void ProfileSettingsViewModel_GyroMouseStickSmoothMethodIndexChanged(object sender, EventArgs e)
        {
            GyroMouseStickWeightAvgPanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
            GyroMouseStickOneEuroPanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ProfileSettingsViewModel_GyroMouseSmoothMethodIndexChanged(object sender, EventArgs e)
        {
            GyroMouseWeightAvgPanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
            GyroMouseOneEuroPanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ProfileSettingsViewModel_ButtonMouseOffsetChanged(object sender,
            EventArgs e)
        {
            MouseOffsetSpeed = RawButtonMouseOffset * OutputMouseSpeed;
        }

        private void ProfileSettingsViewModel_MainColorChanged(object sender, EventArgs e)
        {
            MainColorStringChanged?.Invoke(this, EventArgs.Empty);
            MainColorRChanged?.Invoke(this, EventArgs.Empty);
            MainColorGChanged?.Invoke(this, EventArgs.Empty);
            MainColorBChanged?.Invoke(this, EventArgs.Empty);
            LightbarBrushChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateFlashColor(Color color)
        {
            appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.FlashLed = new DS4Color(color);
            FlashColorChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateMainColor(Color color)
        {
            appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led = new DS4Color(color);
            MainColorChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateLowColor(Color color)
        {
            appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed = new DS4Color(color);

            LowColorChanged?.Invoke(this, EventArgs.Empty);
            LowColorRChanged?.Invoke(this, EventArgs.Empty);
            LowColorGChanged?.Invoke(this, EventArgs.Empty);
            LowColorBChanged?.Invoke(this, EventArgs.Empty);
            LowColorRStringChanged?.Invoke(this, EventArgs.Empty);
            LowColorGStringChanged?.Invoke(this, EventArgs.Empty);
            LowColorBStringChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateForcedColor(Color color)
        {
            if (Device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var dcolor = new DS4Color(color);
                DS4LightBar.forcedColor[Device] = dcolor;
                DS4LightBar.forcedFlash[Device] = 0;
                DS4LightBar.forcelight[Device] = true;
            }
        }

        public void StartForcedColor(Color color)
        {
            if (Device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var dcolor = new DS4Color(color);
                DS4LightBar.forcedColor[Device] = dcolor;
                DS4LightBar.forcedFlash[Device] = 0;
                DS4LightBar.forcelight[Device] = true;
            }
        }

        public void EndForcedColor()
        {
            if (Device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4LightBar.forcedColor[Device] = new DS4Color(0, 0, 0);
                DS4LightBar.forcedFlash[Device] = 0;
                DS4LightBar.forcelight[Device] = false;
            }
        }

        public void UpdateChargingColor(Color color)
        {
            appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.ChargingLed = new DS4Color(color);

            ChargingColorChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateLaunchProgram(string path)
        {
            Global.Instance.Config.LaunchProgram[Device] = path;
            LaunchProgramExistsChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramNameChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramIconChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ResetLauchProgram()
        {
            Global.Instance.Config.LaunchProgram[Device] = string.Empty;
            LaunchProgramExistsChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramNameChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramIconChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateTouchDisInvert(ContextMenu menu)
        {
            var index = 0;
            var triggerList = new List<int>();
            var triggerName = new List<string>();

            foreach (MenuItem item in menu.Items)
            {
                if (item.IsChecked)
                {
                    triggerList.Add(index);
                    triggerName.Add(item.Header.ToString());
                }

                index++;
            }

            if (triggerList.Count == 0)
            {
                triggerList.Add(-1);
                triggerName.Add("None");
            }

            Global.Instance.Config.TouchDisInvertTriggers[Device] = triggerList.ToArray();
            TouchDisInvertString = string.Join(", ", triggerName.ToArray());
        }

        public void PopulateTouchDisInver(ContextMenu menu)
        {
            var triggers = Global.Instance.Config.TouchDisInvertTriggers[Device];
            var itemCount = menu.Items.Count;
            var triggerName = new List<string>();
            foreach (var trigid in triggers)
                if (trigid >= 0 && trigid < itemCount - 1)
                {
                    var current = menu.Items[trigid] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add(current.Header.ToString());
                }
                else if (trigid == -1)
                {
                    triggerName.Add("None");
                    break;
                }

            if (triggerName.Count == 0) triggerName.Add("None");

            TouchDisInvertString = string.Join(", ", triggerName.ToArray());
        }

        public void UpdateGyroMouseTrig(ContextMenu menu, bool alwaysOnChecked)
        {
            var index = 0;
            var triggerList = new List<int>();
            var triggerName = new List<string>();

            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;
            if (alwaysOnChecked)
            {
                for (var i = 0; i < itemCount - 1; i++)
                {
                    var item = menu.Items[i] as MenuItem;
                    item.IsChecked = false;
                }
            }
            else
            {
                alwaysOnItem.IsChecked = false;
                foreach (MenuItem item in menu.Items)
                {
                    if (item.IsChecked)
                    {
                        triggerList.Add(index);
                        triggerName.Add(item.Header.ToString());
                    }

                    index++;
                }
            }

            if (triggerList.Count == 0)
            {
                triggerList.Add(-1);
                triggerName.Add("Always On");
                alwaysOnItem.IsChecked = true;
            }

            ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SATriggers =
                string.Join(",", triggerList.ToArray());
            GyroMouseTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void PopulateGyroMouseTrig(ContextMenu menu)
        {
            var triggers = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SATriggers.Split(',');
            var itemCount = menu.Items.Count;
            var triggerName = new List<string>();
            foreach (var trig in triggers)
            {
                var valid = int.TryParse(trig, out var trigid);
                if (valid && trigid >= 0 && trigid < itemCount - 1)
                {
                    var current = menu.Items[trigid] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add(current.Header.ToString());
                }
                else if (valid && trigid == -1)
                {
                    var current = menu.Items[itemCount - 1] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add("Always On");
                    break;
                }
            }

            if (triggerName.Count == 0)
            {
                var current = menu.Items[itemCount - 1] as MenuItem;
                current.IsChecked = true;
                triggerName.Add("Always On");
            }

            GyroMouseTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void UpdateGyroMouseStickTrig(ContextMenu menu, bool alwaysOnChecked)
        {
            var index = 0;
            var triggerList = new List<int>();
            var triggerName = new List<string>();

            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;
            if (alwaysOnChecked)
            {
                for (var i = 0; i < itemCount - 1; i++)
                {
                    var item = menu.Items[i] as MenuItem;
                    item.IsChecked = false;
                }
            }
            else
            {
                alwaysOnItem.IsChecked = false;
                foreach (MenuItem item in menu.Items)
                {
                    if (item.IsChecked)
                    {
                        triggerList.Add(index);
                        triggerName.Add(item.Header.ToString());
                    }

                    index++;
                }
            }

            if (triggerList.Count == 0)
            {
                triggerList.Add(-1);
                triggerName.Add("Always On");
                alwaysOnItem.IsChecked = true;
            }

            Global.Instance.Config.SAMouseStickTriggers[Device] = string.Join(",", triggerList.ToArray());
            GyroMouseStickTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void PopulateGyroMouseStickTrig(ContextMenu menu)
        {
            var triggers = Global.Instance.Config.SAMouseStickTriggers[Device].Split(',');
            var itemCount = menu.Items.Count;
            var triggerName = new List<string>();
            foreach (var trig in triggers)
            {
                var valid = int.TryParse(trig, out var trigid);
                if (valid && trigid >= 0 && trigid < itemCount - 1)
                {
                    var current = menu.Items[trigid] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add(current.Header.ToString());
                }
                else if (valid && trigid == -1)
                {
                    var current = menu.Items[itemCount - 1] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add("Always On");
                    break;
                }
            }

            if (triggerName.Count == 0)
            {
                var current = menu.Items[itemCount - 1] as MenuItem;
                current.IsChecked = true;
                triggerName.Add("Always On");
            }

            GyroMouseStickTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void UpdateGyroSwipeTrig(ContextMenu menu, bool alwaysOnChecked)
        {
            var index = 0;
            var triggerList = new List<int>();
            var triggerName = new List<string>();

            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;
            if (alwaysOnChecked)
            {
                for (var i = 0; i < itemCount - 1; i++)
                {
                    var item = menu.Items[i] as MenuItem;
                    item.IsChecked = false;
                }
            }
            else
            {
                alwaysOnItem.IsChecked = false;
                foreach (MenuItem item in menu.Items)
                {
                    if (item.IsChecked)
                    {
                        triggerList.Add(index);
                        triggerName.Add(item.Header.ToString());
                    }

                    index++;
                }
            }

            if (triggerList.Count == 0)
            {
                triggerList.Add(-1);
                triggerName.Add("Always On");
                alwaysOnItem.IsChecked = true;
            }

            Global.Instance.Config.GyroSwipeInfo[Device].triggers = string.Join(",", triggerList.ToArray());
            GyroSwipeTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void PopulateGyroSwipeTrig(ContextMenu menu)
        {
            var triggers = Global.Instance.Config.GyroSwipeInfo[Device].triggers.Split(',');
            var itemCount = menu.Items.Count;
            var triggerName = new List<string>();
            foreach (var trig in triggers)
            {
                var valid = int.TryParse(trig, out var trigid);
                if (valid && trigid >= 0 && trigid < itemCount - 1)
                {
                    var current = menu.Items[trigid] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add(current.Header.ToString());
                }
                else if (valid && trigid == -1)
                {
                    var current = menu.Items[itemCount - 1] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add("Always On");
                    break;
                }
            }

            if (triggerName.Count == 0)
            {
                var current = menu.Items[itemCount - 1] as MenuItem;
                current.IsChecked = true;
                triggerName.Add("Always On");
            }

            GyroSwipeTrigDisplay = string.Join(", ", triggerName.ToArray());
        }


        public void UpdateGyroControlsTrig(ContextMenu menu, bool alwaysOnChecked)
        {
            var index = 0;
            var triggerList = new List<int>();
            var triggerName = new List<string>();

            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;
            if (alwaysOnChecked)
            {
                for (var i = 0; i < itemCount - 1; i++)
                {
                    var item = menu.Items[i] as MenuItem;
                    item.IsChecked = false;
                }
            }
            else
            {
                alwaysOnItem.IsChecked = false;
                foreach (MenuItem item in menu.Items)
                {
                    if (item.IsChecked)
                    {
                        triggerList.Add(index);
                        triggerName.Add(item.Header.ToString());
                    }

                    index++;
                }
            }

            if (triggerList.Count == 0)
            {
                triggerList.Add(-1);
                triggerName.Add("Always On");
                alwaysOnItem.IsChecked = true;
            }

            ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroControlsInfo.Triggers =
                string.Join(",", triggerList.ToArray());
            GyroControlsTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void PopulateGyroControlsTrig(ContextMenu menu)
        {
            var triggers = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroControlsInfo.Triggers
                .Split(',');
            var itemCount = menu.Items.Count;
            var triggerName = new List<string>();
            foreach (var trig in triggers)
            {
                var valid = int.TryParse(trig, out var trigid);
                if (valid && trigid >= 0 && trigid < itemCount - 1)
                {
                    var current = menu.Items[trigid] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add(current.Header.ToString());
                }
                else if (valid && trigid == -1)
                {
                    var current = menu.Items[itemCount - 1] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add("Always On");
                    break;
                }
            }

            if (triggerName.Count == 0)
            {
                var current = menu.Items[itemCount - 1] as MenuItem;
                current.IsChecked = true;
                triggerName.Add("Always On");
            }

            GyroControlsTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        private int CalculateOutputMouseSpeed(int mouseSpeed)
        {
            var result = mouseSpeed * Mapping.MOUSESPEEDFACTOR;
            return result;
        }

        public void LaunchCurveEditor(string customDefinition)
        {
            // Custom curve editor web link clicked. Open the bezier curve editor web app usign the default browser app and pass on current custom definition as a query string parameter.
            // The Process.Start command using HTML page doesn't support query parameters, so if there is a custom curve definition then lookup the default browser executable name from a sysreg.
            var defaultBrowserCmd = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(customDefinition))
                {
                    var progId = string.Empty;
                    using (var userChoiceKey = Registry.CurrentUser.OpenSubKey(
                        "Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\http\\UserChoice"))
                    {
                        progId = userChoiceKey?.GetValue("Progid")?.ToString();
                    }

                    if (!string.IsNullOrEmpty(progId))
                    {
                        using (var browserPathCmdKey =
                            Registry.ClassesRoot.OpenSubKey($"{progId}\\shell\\open\\command"))
                        {
                            defaultBrowserCmd = browserPathCmdKey?.GetValue(null).ToString().ToLower();
                        }

                        if (!string.IsNullOrEmpty(defaultBrowserCmd))
                        {
                            var iStartPos = defaultBrowserCmd[0] == '"' ? 1 : 0;
                            defaultBrowserCmd = defaultBrowserCmd.Substring(iStartPos,
                                defaultBrowserCmd.LastIndexOf(".exe") + 4 - iStartPos);
                            if (Path.GetFileName(defaultBrowserCmd) == "launchwinapp.exe")
                                defaultBrowserCmd = string.Empty;
                        }

                        // Fallback to IE executable if the default browser HTML shell association is for some reason missing or is not set
                        if (string.IsNullOrEmpty(defaultBrowserCmd))
                            defaultBrowserCmd = "C:\\Program Files\\Internet Explorer\\iexplore.exe";

                        if (!File.Exists(defaultBrowserCmd))
                            defaultBrowserCmd = string.Empty;
                    }
                }

                // Launch custom bezier editor webapp using a default browser executable command or via a default shell command. The default shell exeution doesn't support query parameters.
                if (!string.IsNullOrEmpty(defaultBrowserCmd))
                {
                    Process.Start(defaultBrowserCmd,
                        $"\"file:///{Global.ExecutableDirectory}\\BezierCurveEditor\\index.html?curve={customDefinition.Replace(" ", "")}\"");
                }
                else
                {
                    var startInfo =
                        new ProcessStartInfo($"{Global.ExecutableDirectory}\\BezierCurveEditor\\index.html");
                    startInfo.UseShellExecute = true;
                    using (var temp = Process.Start(startInfo))
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogToGui(
                    $"ERROR. Failed to open {Global.ExecutableDirectory}\\BezierCurveEditor\\index.html web app. Check that the web file exits or launch it outside of DS4Windows application. {ex.Message}",
                    true);
            }
        }

        public void UpdateLateProperties()
        {
            tempControllerIndex = ControllerTypeIndex;
            Global.OutDevTypeTemp[Device] = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).OutputDeviceType;
            TempBTPollRateIndex = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).BluetoothPollRate;
            outputMouseSpeed = CalculateOutputMouseSpeed(ButtonMouseSensitivity);
            mouseOffsetSpeed = RawButtonMouseOffset * outputMouseSpeed;
            gyroMouseSmoothMethodIndex = FindGyroMouseSmoothMethodIndex();
            gyroMouseStickSmoothMethodIndex = FindGyroMouseStickSmoothMethodIndex();
        }
    }

    public class PresetMenuHelper
    {
        public enum ControlSelection : uint
        {
            None,
            LeftStick,
            RightStick,
            DPad,
            FaceButtons
        }

        private readonly int deviceNum;

        public PresetMenuHelper(int device)
        {
            deviceNum = device;
        }

        public Dictionary<ControlSelection, string> PresetInputLabelDict { get; } = new()
        {
            [ControlSelection.None] = "None",
            [ControlSelection.DPad] = "DPad",
            [ControlSelection.LeftStick] = "Left Stick",
            [ControlSelection.RightStick] = "Right Stick",
            [ControlSelection.FaceButtons] = "Face Buttons"
        };

        public string PresetInputLabel => PresetInputLabelDict[HighlightControl];

        public ControlSelection HighlightControl { get; private set; } = ControlSelection.None;

        public ControlSelection PresetTagIndex(DS4Controls control)
        {
            var controlInput = ControlSelection.None;
            switch (control)
            {
                case DS4Controls.DpadUp:
                case DS4Controls.DpadDown:
                case DS4Controls.DpadLeft:
                case DS4Controls.DpadRight:
                    controlInput = ControlSelection.DPad;
                    break;
                case DS4Controls.LXNeg:
                case DS4Controls.LXPos:
                case DS4Controls.LYNeg:
                case DS4Controls.LYPos:
                case DS4Controls.L3:
                    controlInput = ControlSelection.LeftStick;
                    break;
                case DS4Controls.RXNeg:
                case DS4Controls.RXPos:
                case DS4Controls.RYNeg:
                case DS4Controls.RYPos:
                case DS4Controls.R3:
                    controlInput = ControlSelection.RightStick;
                    break;
                case DS4Controls.Cross:
                case DS4Controls.Circle:
                case DS4Controls.Triangle:
                case DS4Controls.Square:
                    controlInput = ControlSelection.FaceButtons;
                    break;
            }


            return controlInput;
        }

        public void SetHighlightControl(DS4Controls control)
        {
            var controlInput = PresetTagIndex(control);
            HighlightControl = controlInput;
        }

        public List<DS4Controls> ModifySettingWithPreset(int baseTag, int subTag)
        {
            var actionBtns = new List<object>(5);
            var inputControls = new List<DS4Controls>(5);
            if (baseTag == 0)
                actionBtns.AddRange(new object[5]
                {
                    null, null, null, null, null
                });
            else if (baseTag == 1)
                switch (subTag)
                {
                    case 0:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadUp, X360Controls.DpadDown,
                            X360Controls.DpadLeft, X360Controls.DpadRight, X360Controls.Unbound
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadDown, X360Controls.DpadUp,
                            X360Controls.DpadRight, X360Controls.DpadLeft, X360Controls.Unbound
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadUp, X360Controls.DpadDown,
                            X360Controls.DpadRight, X360Controls.DpadLeft, X360Controls.Unbound
                        });
                        break;
                    case 3:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadDown, X360Controls.DpadUp,
                            X360Controls.DpadLeft, X360Controls.DpadRight, X360Controls.Unbound
                        });
                        break;
                    case 4:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadRight, X360Controls.DpadLeft,
                            X360Controls.DpadUp, X360Controls.DpadDown, X360Controls.Unbound
                        });
                        break;
                    case 5:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadLeft, X360Controls.DpadRight,
                            X360Controls.DpadDown, X360Controls.DpadUp, X360Controls.Unbound
                        });
                        break;
                }
            else if (baseTag == 2)
                switch (subTag)
                {
                    case 0:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LYNeg, X360Controls.LYPos,
                            X360Controls.LXNeg, X360Controls.LXPos, X360Controls.LS
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LYPos, X360Controls.LYNeg,
                            X360Controls.LXPos, X360Controls.LXNeg, X360Controls.LS
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LYNeg, X360Controls.LYPos,
                            X360Controls.LXPos, X360Controls.LXNeg, X360Controls.LS
                        });
                        break;
                    case 3:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LYPos, X360Controls.LYNeg,
                            X360Controls.LXNeg, X360Controls.LXPos, X360Controls.LS
                        });
                        break;
                    case 4:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LXPos, X360Controls.LXNeg,
                            X360Controls.LYNeg, X360Controls.LYPos, X360Controls.LS
                        });
                        break;
                    case 5:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LXNeg, X360Controls.LXPos,
                            X360Controls.LYPos, X360Controls.LYNeg, X360Controls.LS
                        });
                        break;
                }
            else if (baseTag == 3)
                switch (subTag)
                {
                    case 0:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RYNeg, X360Controls.RYPos,
                            X360Controls.RXNeg, X360Controls.RXPos, X360Controls.RS
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RYPos, X360Controls.RYNeg,
                            X360Controls.RXPos, X360Controls.RXNeg, X360Controls.RS
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RYNeg, X360Controls.RYPos,
                            X360Controls.RXPos, X360Controls.RXNeg, X360Controls.RS
                        });
                        break;
                    case 3:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RYPos, X360Controls.RYNeg,
                            X360Controls.RXNeg, X360Controls.RXPos, X360Controls.RS
                        });
                        break;
                    case 4:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RXPos, X360Controls.RXNeg,
                            X360Controls.RYNeg, X360Controls.RYPos, X360Controls.RS
                        });
                        break;
                    case 5:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RXNeg, X360Controls.RXPos,
                            X360Controls.RYPos, X360Controls.RYNeg, X360Controls.RS
                        });
                        break;
                }
            else if (baseTag == 4)
                switch (subTag)
                {
                    case 0:
                        // North, South, West, East
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.Y, X360Controls.A, X360Controls.X, X360Controls.B, X360Controls.Unbound
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.B, X360Controls.X, X360Controls.Y, X360Controls.A, X360Controls.Unbound
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.X, X360Controls.B, X360Controls.A, X360Controls.Y, X360Controls.Unbound
                        });
                        break;
                }
            else if (baseTag == 5)
                switch (subTag)
                {
                    case 0:
                        // North, South, West, East
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.W), KeyInterop.VirtualKeyFromKey(Key.S),
                            KeyInterop.VirtualKeyFromKey(Key.A), KeyInterop.VirtualKeyFromKey(Key.D),
                            X360Controls.Unbound
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.D), KeyInterop.VirtualKeyFromKey(Key.A),
                            KeyInterop.VirtualKeyFromKey(Key.W), KeyInterop.VirtualKeyFromKey(Key.S),
                            X360Controls.Unbound
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.A), KeyInterop.VirtualKeyFromKey(Key.D),
                            KeyInterop.VirtualKeyFromKey(Key.S), KeyInterop.VirtualKeyFromKey(Key.W),
                            X360Controls.Unbound
                        });
                        break;
                }
            else if (baseTag == 6)
                switch (subTag)
                {
                    case 0:
                        // North, South, West, East
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.Up), KeyInterop.VirtualKeyFromKey(Key.Down),
                            KeyInterop.VirtualKeyFromKey(Key.Left), KeyInterop.VirtualKeyFromKey(Key.Right),
                            X360Controls.Unbound
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.Right), KeyInterop.VirtualKeyFromKey(Key.Left),
                            KeyInterop.VirtualKeyFromKey(Key.Up), KeyInterop.VirtualKeyFromKey(Key.Down),
                            X360Controls.Unbound
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.Left), KeyInterop.VirtualKeyFromKey(Key.Right),
                            KeyInterop.VirtualKeyFromKey(Key.Down), KeyInterop.VirtualKeyFromKey(Key.Up),
                            X360Controls.Unbound
                        });
                        break;
                }
            else if (baseTag == 7)
                switch (subTag)
                {
                    case 0:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseUp, X360Controls.MouseDown,
                            X360Controls.MouseLeft, X360Controls.MouseRight,
                            X360Controls.Unbound
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseDown, X360Controls.MouseUp,
                            X360Controls.MouseRight, X360Controls.MouseLeft,
                            X360Controls.Unbound
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseUp, X360Controls.MouseDown,
                            X360Controls.MouseRight, X360Controls.MouseLeft,
                            X360Controls.Unbound
                        });
                        break;
                    case 3:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseDown, X360Controls.MouseUp,
                            X360Controls.MouseLeft, X360Controls.MouseRight,
                            X360Controls.Unbound
                        });
                        break;
                    case 4:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseRight, X360Controls.MouseLeft,
                            X360Controls.MouseUp, X360Controls.MouseDown,
                            X360Controls.Unbound
                        });
                        break;
                    case 5:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseLeft, X360Controls.MouseRight,
                            X360Controls.MouseDown, X360Controls.MouseUp,
                            X360Controls.Unbound
                        });
                        break;
                }
            else if (baseTag == 8)
                actionBtns.AddRange(new object[5]
                {
                    X360Controls.Unbound, X360Controls.Unbound,
                    X360Controls.Unbound, X360Controls.Unbound,
                    X360Controls.Unbound
                });


            switch (HighlightControl)
            {
                case ControlSelection.DPad:
                    inputControls.AddRange(new DS4Controls[4]
                    {
                        DS4Controls.DpadUp, DS4Controls.DpadDown,
                        DS4Controls.DpadLeft, DS4Controls.DpadRight
                    });
                    break;
                case ControlSelection.LeftStick:
                    inputControls.AddRange(new DS4Controls[5]
                    {
                        DS4Controls.LYNeg, DS4Controls.LYPos,
                        DS4Controls.LXNeg, DS4Controls.LXPos, DS4Controls.L3
                    });
                    break;
                case ControlSelection.RightStick:
                    inputControls.AddRange(new DS4Controls[5]
                    {
                        DS4Controls.RYNeg, DS4Controls.RYPos,
                        DS4Controls.RXNeg, DS4Controls.RXPos, DS4Controls.R3
                    });
                    break;
                case ControlSelection.FaceButtons:
                    inputControls.AddRange(new DS4Controls[4]
                    {
                        DS4Controls.Triangle, DS4Controls.Cross,
                        DS4Controls.Square, DS4Controls.Circle
                    });
                    break;
                case ControlSelection.None:
                default:
                    break;
            }

            var idx = 0;
            foreach (var dsControl in inputControls)
            {
                var setting = Global.Instance.Config.GetDs4ControllerSetting(deviceNum, dsControl);
                setting.Reset();
                if (idx < actionBtns.Count && actionBtns[idx] != null)
                {
                    var outAct = actionBtns[idx];
                    var defaultControl = Global.DefaultButtonMapping[(int)dsControl];
                    if (!(outAct is X360Controls) || defaultControl != (X360Controls)outAct)
                    {
                        setting.UpdateSettings(false, outAct, null, DS4KeyType.None);
                        Global.RefreshActionAlias(setting, false);
                    }
                }

                idx++;
            }

            return inputControls;
        }
    }

    public class TriggerModeChoice
    {
        public TriggerMode mode;

        public TriggerModeChoice(string name, TriggerMode mode)
        {
            DisplayName = name;
            this.mode = mode;
        }

        public string DisplayName { get; set; }

        public TriggerMode Mode
        {
            get => mode;
            set => mode = value;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public class TwoStageChoice
    {
        public TwoStageChoice(string name, TwoStageTriggerMode mode)
        {
            DisplayName = name;
            Mode = mode;
        }

        public string DisplayName { get; set; }

        public TwoStageTriggerMode Mode { get; set; }
    }

    public class TriggerEffectChoice
    {
        public TriggerEffectChoice(string name, TriggerEffects mode)
        {
            DisplayName = name;
            Mode = mode;
        }

        public string DisplayName { get; set; }

        public TriggerEffects Mode { get; set; }
    }
}