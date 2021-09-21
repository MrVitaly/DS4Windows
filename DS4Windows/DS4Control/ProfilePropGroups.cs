﻿using System;
using Sensorit.Base;

namespace DS4Windows
{
    public class SquareStickInfo
    {
        public bool LSMode { get; set; }
        public bool RSMode { get; set; }
        public double LSRoundness { get; set; } = 5.0;
        public double RSRoundness { get; set; } = 5.0;
    }

    public class StickDeadZoneInfo
    {
        public enum DeadZoneType : ushort
        {
            Radial,
            Axial
        }

        public const int DEFAULT_DEADZONE = 10;
        public const int DEFAULT_ANTIDEADZONE = 20;
        public const int DEFAULT_MAXZONE = 100;
        public const double DEFAULT_MAXOUTPUT = 100.0;
        public const bool DEFAULT_MAXOUTPUT_FORCE = false;
        public const int DEFAULT_FUZZ = 0;
        public const DeadZoneType DEFAULT_DEADZONE_TYPE = DeadZoneType.Radial;
        public const double DEFAULT_VERTICAL_SCALE = 100.0;
        public const double DEFAULT_OUTER_BIND_DEAD = 75.0;
        public const bool DEFAULT_OUTER_BIND_INVERT = false;

        public class AxisDeadZoneInfo
        {
            // DeadZone value from 0-127 (old bad convention)
            public int DeadZone { get; set; } = DEFAULT_DEADZONE;
            public int AntiDeadZone { get; set; } = DEFAULT_ANTIDEADZONE;
            public int MaxZone { get; set; } = DEFAULT_MAXZONE;
            public double MaxOutput { get; set; } = DEFAULT_MAXOUTPUT;

            public void Reset()
            {
                DeadZone = DEFAULT_DEADZONE;
                AntiDeadZone = DEFAULT_ANTIDEADZONE;
                MaxZone = DEFAULT_MAXZONE;
                MaxOutput = DEFAULT_MAXOUTPUT;
            }
        }

        // DeadZone value from 0-127 (old bad convention)
        public int DeadZone { get; set; }
        public int AntiDeadZone { get; set; }
        public int MaxZone  { get; set; } = DEFAULT_MAXZONE;
        public double MaxOutput { get; set; } = DEFAULT_MAXOUTPUT;
        public bool MaxOutputForce { get; set; } = DEFAULT_MAXOUTPUT_FORCE;
        public int Fuzz { get; set; } = DEFAULT_FUZZ;
        public double VerticalScale { get; set; } = DEFAULT_VERTICAL_SCALE;
        public DeadZoneType DZType { get; set; } = DEFAULT_DEADZONE_TYPE;
        public double OuterBindDeadZone { get; set; } = DEFAULT_OUTER_BIND_DEAD;
        public bool OuterBindInvert { get; set; } = DEFAULT_OUTER_BIND_INVERT;
        public AxisDeadZoneInfo XAxisDeadInfo { get; set; } = new();
        public AxisDeadZoneInfo YAxisDeadInfo { get; set; } = new();

        public void Reset()
        {
            DeadZone = 0;
            AntiDeadZone = 0;
            MaxZone = DEFAULT_MAXZONE;
            MaxOutput = DEFAULT_MAXOUTPUT;
            MaxOutputForce = DEFAULT_MAXOUTPUT_FORCE;

            Fuzz = DEFAULT_FUZZ;
            VerticalScale = DEFAULT_VERTICAL_SCALE;
            DZType = DEFAULT_DEADZONE_TYPE;
            OuterBindDeadZone = DEFAULT_OUTER_BIND_DEAD;
            OuterBindInvert = DEFAULT_OUTER_BIND_INVERT;
            XAxisDeadInfo.Reset();
            YAxisDeadInfo.Reset();
        }
    }

    public class StickAntiSnapbackInfo
    {
        public const double DEFAULT_DELTA = 135;
        public const int DEFAULT_TIMEOUT = 50;
        public const bool DEFAULT_ENABLED = false;

        public bool Enabled { get; set; } = DEFAULT_ENABLED;
        public double Delta { get; set; } = DEFAULT_DELTA;
        public int Timeout { get; set; } = DEFAULT_TIMEOUT;
    }

    public class TriggerDeadZoneZInfo
    {
        // Trigger deadzone is expressed in axis units (bad old convention)
        public byte deadZone;

        public byte DeadZone
        {
            get => deadZone;
            set
            {
                if (deadZone == value) return;
                deadZone = value;
                DeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeadZoneChanged;

        public int AntiDeadZone { get; set; }
        public int maxZone = 100;
        public int MaxZone
        {
            get => maxZone;
            set
            {
                if (maxZone == value) return;
                maxZone = value;
                MaxZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MaxZoneChanged;

        public double maxOutput = 100.0;

        public double MaxOutput
        {
            get => maxOutput;
            set
            {
                if (maxOutput == value) return;
                maxOutput = value;
                MaxOutputChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MaxOutputChanged;

        public void Reset()
        {
            deadZone = 0;
            AntiDeadZone = 0;
            MaxZone = 100;
            MaxOutput = 100.0;
        }

        public void ResetEvents()
        {
            MaxZoneChanged = null;
            MaxOutputChanged = null;
            DeadZoneChanged = null;
        }
    }

    public class GyroMouseInfo
    {
        public enum SmoothingMethod : byte
        {
            None,
            OneEuro,
            WeightedAverage,
        }

        public const double DEFAULT_MINCUTOFF = 1.0;
        public const double DEFAULT_BETA = 0.7;
        public const string DEFAULT_SMOOTH_TECHNIQUE = "one-euro";
        public const double DEFAULT_MIN_THRESHOLD = 1.0;

        public bool enableSmoothing = false;
        public double smoothingWeight = 0.5;
        public SmoothingMethod smoothingMethod;

        public double minCutoff = DEFAULT_MINCUTOFF;
        public double beta = DEFAULT_BETA;
        public double minThreshold = DEFAULT_MIN_THRESHOLD;

        public delegate void GyroMouseInfoEventHandler(GyroMouseInfo sender, EventArgs args);

        public double MinCutoff
        {
            get => minCutoff;
            set
            {
                if (minCutoff == value) return;
                minCutoff = value;
                MinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseInfoEventHandler MinCutoffChanged;

        public double Beta
        {
            get => beta;
            set
            {
                if (beta == value) return;
                beta = value;
                BetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseInfoEventHandler BetaChanged;

        public void Reset()
        {
            minCutoff = DEFAULT_MINCUTOFF;
            beta = DEFAULT_BETA;
            enableSmoothing = false;
            smoothingMethod = SmoothingMethod.None;
            smoothingWeight = 0.5;
            minThreshold = DEFAULT_MIN_THRESHOLD;
        }

        public void ResetSmoothing()
        {
            enableSmoothing = false;
            ResetSmoothingMethods();
        }

        public void ResetSmoothingMethods()
        {
            smoothingMethod = SmoothingMethod.None;
        }

        public void DetermineSmoothMethod(string identier)
        {
            ResetSmoothingMethods();

            switch (identier)
            {
                case "weighted-average":
                    smoothingMethod = SmoothingMethod.WeightedAverage;
                    break;
                case "one-euro":
                    smoothingMethod = SmoothingMethod.OneEuro;
                    break;
                default:
                    smoothingMethod = SmoothingMethod.None;
                    break;
            }
        }

        public string SmoothMethodIdentifier()
        {
            string result = "none";
            if (smoothingMethod == SmoothingMethod.OneEuro)
            {
                result = "one-euro";
            }
            else if (smoothingMethod == SmoothingMethod.WeightedAverage)
            {
                result = "weighted-average";
            }

            return result;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) =>
            {
                euroFilter.Beta = beta;
            };

            MinCutoffChanged += (sender, args) =>
            {
                euroFilter.MinCutoff = minCutoff;
            };
        }

        public void RemoveRefreshEvents()
        {
            BetaChanged = null;
            MinCutoffChanged = null;
        }
    }

    public class GyroMouseStickInfo
    {
        public enum SmoothingMethod : byte
        {
            None,
            OneEuro,
            WeightedAverage,
        }

        public enum OutputStick : byte
        {
            None,
            LeftStick,
            RightStick,
        }

        public enum OutputStickAxes : byte
        {
            None,
            XY,
            X,
            Y
        }

        public const double DEFAULT_MINCUTOFF = 0.4;
        public const double DEFAULT_BETA = 0.7;
        public const string DEFAULT_SMOOTH_TECHNIQUE = "one-euro";
        public const OutputStick DEFAULT_OUTPUT_STICK = OutputStick.RightStick;
        public const OutputStickAxes DEFAULT_OUTPUT_STICK_AXES = OutputStickAxes.XY;

        public int deadZone;
        public int maxZone;
        public double antiDeadX;
        public double antiDeadY;
        public int vertScale;
        public bool maxOutputEnabled;
        public double maxOutput = 100.0;
        // Flags representing invert axis choices
        public uint inverted;
        public bool useSmoothing;
        public double smoothWeight;
        public SmoothingMethod smoothingMethod;
        public double minCutoff = DEFAULT_MINCUTOFF;
        public double beta = DEFAULT_BETA;
        public OutputStick outputStick = DEFAULT_OUTPUT_STICK;
        public OutputStickAxes outputStickDir = DEFAULT_OUTPUT_STICK_AXES;

        public delegate void GyroMouseStickInfoEventHandler(GyroMouseStickInfo sender,
            EventArgs args);


        public double MinCutoff
        {
            get => minCutoff;
            set
            {
                if (minCutoff == value) return;
                minCutoff = value;
                MinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseStickInfoEventHandler MinCutoffChanged;

        public double Beta
        {
            get => beta;
            set
            {
                if (beta == value) return;
                beta = value;
                BetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseStickInfoEventHandler BetaChanged;

        public void Reset()
        {
            deadZone = 30; maxZone = 830;
            antiDeadX = 0.4; antiDeadY = 0.4;
            inverted = 0; vertScale = 100;
            maxOutputEnabled = false; maxOutput = 100.0;
            outputStick = DEFAULT_OUTPUT_STICK;
            outputStickDir = DEFAULT_OUTPUT_STICK_AXES;

            minCutoff = DEFAULT_MINCUTOFF;
            beta = DEFAULT_BETA;
            smoothingMethod = SmoothingMethod.None;
            useSmoothing = false;
            smoothWeight = 0.5;
        }

        public void ResetSmoothing()
        {
            useSmoothing = false;
            ResetSmoothingMethods();
        }

        public void ResetSmoothingMethods()
        {
            smoothingMethod = SmoothingMethod.None;
        }

        public void DetermineSmoothMethod(string identier)
        {
            ResetSmoothingMethods();

            switch (identier)
            {
                case "weighted-average":
                    smoothingMethod = SmoothingMethod.WeightedAverage;
                    break;
                case "one-euro":
                    smoothingMethod = SmoothingMethod.OneEuro;
                    break;
                default:
                    smoothingMethod = SmoothingMethod.None;
                    break;
            }
        }

        public string SmoothMethodIdentifier()
        {
            string result = "none";
            switch (smoothingMethod)
            {
                case SmoothingMethod.WeightedAverage:
                    result = "weighted-average";
                    break;
                case SmoothingMethod.OneEuro:
                    result = "one-euro";
                    break;
                default:
                    break;
            }

            return result;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) =>
            {
                euroFilter.Beta = beta;
            };

            MinCutoffChanged += (sender, args) =>
            {
                euroFilter.MinCutoff = minCutoff;
            };
        }

        public void RemoveRefreshEvents()
        {
            BetaChanged = null;
            MinCutoffChanged = null;
        }

        public bool OutputHorizontal()
        {
            return outputStickDir == OutputStickAxes.XY ||
                outputStickDir == OutputStickAxes.X;
        }

        public bool OutputVertical()
        {
            return outputStickDir == OutputStickAxes.XY ||
                outputStickDir == OutputStickAxes.Y;
        }
    }

    public class GyroDirectionalSwipeInfo
    {
        public enum XAxisSwipe : ushort
        {
            Yaw,
            Roll,
        }

        public const string DEFAULT_TRIGGERS = "-1";
        public const int DEFAULT_GYRO_DIR_SPEED = 80; // degrees per second
        public const bool DEFAULT_TRIGGER_COND = true;
        public const bool DEFAULT_TRIGGER_TURNS = true;
        public const XAxisSwipe DEFAULT_X_AXIS = XAxisSwipe.Yaw;
        public const int DEFAULT_DELAY_TIME = 0;

        public int deadzoneX = DEFAULT_GYRO_DIR_SPEED;
        public int deadzoneY = DEFAULT_GYRO_DIR_SPEED;
        public string triggers = DEFAULT_TRIGGERS;
        public bool triggerCond = DEFAULT_TRIGGER_COND;
        public bool triggerTurns = DEFAULT_TRIGGER_TURNS;
        public XAxisSwipe xAxis = DEFAULT_X_AXIS;
        public int delayTime = DEFAULT_DELAY_TIME;

        public void Reset()
        {
            deadzoneX = DEFAULT_GYRO_DIR_SPEED;
            deadzoneY = DEFAULT_GYRO_DIR_SPEED;
            triggers = DEFAULT_TRIGGERS;
            triggerCond = DEFAULT_TRIGGER_COND;
            triggerTurns = DEFAULT_TRIGGER_TURNS;
            xAxis = DEFAULT_X_AXIS;
            delayTime = DEFAULT_DELAY_TIME;
        }
    }

    public class GyroControlsInfo
    {
        public const string DEFAULT_TRIGGERS = "-1";
        public const bool DEFAULT_TRIGGER_COND = true;
        public const bool DEFAULT_TRIGGER_TURNS = true;
        public const bool DEFAULT_TRIGGER_TOGGLE = false;

        public string Triggers { get; set; } = DEFAULT_TRIGGERS;
        public bool TriggerCond { get; set; } = DEFAULT_TRIGGER_COND;
        public bool TriggerTurns { get; set; } = DEFAULT_TRIGGER_TURNS;
        public bool TriggerToggle { get; set; } = DEFAULT_TRIGGER_TOGGLE;

        public void Reset()
        {
            Triggers = DEFAULT_TRIGGERS;
            TriggerCond = DEFAULT_TRIGGER_COND;
            TriggerTurns = DEFAULT_TRIGGER_TURNS;
            TriggerToggle = DEFAULT_TRIGGER_TOGGLE;
        }
    }

    public class ButtonMouseInfo
    {
        //public const double MOUSESTICKANTIOFFSET = 0.0128;
        public const double MOUSESTICKANTIOFFSET = 0.008;
        public const int DEFAULT_BUTTON_SENS = 25;
        public const double DEFAULT_BUTTON_VERTICAL_SCALE = 1.0;
        public const int DEFAULT_TEMP_SENS = -1;

        public int buttonSensitivity = DEFAULT_BUTTON_SENS;
        public int ButtonSensitivity
        {
            get => buttonSensitivity;
            set
            {
                if (buttonSensitivity == value) return;
                buttonSensitivity = value;
                ButtonMouseInfoChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ButtonMouseInfoChanged;

        public bool mouseAccel;
        public int activeButtonSensitivity = DEFAULT_BUTTON_SENS;
        public int tempButtonSensitivity = DEFAULT_TEMP_SENS;
        public double mouseVelocityOffset = MOUSESTICKANTIOFFSET;
        public double buttonVerticalScale = DEFAULT_BUTTON_VERTICAL_SCALE;

        public ButtonMouseInfo()
        {
            ButtonMouseInfoChanged += ButtonMouseInfo_ButtonMouseInfoChanged;
        }

        private void ButtonMouseInfo_ButtonMouseInfoChanged(object sender, EventArgs e)
        {
            if (tempButtonSensitivity == DEFAULT_TEMP_SENS)
            {
                activeButtonSensitivity = buttonSensitivity;
            }
        }

        public void SetActiveButtonSensitivity(int sens)
        {
            activeButtonSensitivity = sens;
        }

        public void Reset()
        {
            buttonSensitivity = DEFAULT_BUTTON_SENS;
            mouseAccel = false;
            activeButtonSensitivity = DEFAULT_BUTTON_SENS;
            tempButtonSensitivity = DEFAULT_TEMP_SENS;
            mouseVelocityOffset = MOUSESTICKANTIOFFSET;
            buttonVerticalScale = DEFAULT_BUTTON_VERTICAL_SCALE;
        }
    }

    public enum LightbarMode : uint
    {
        None,
        DS4Win,
        Passthru,
    }

    public class LightbarDS4WinInfo
    {
        public bool UseCustomLed { get; set; }

        public bool LedAsBattery { get; set; }

        public DS4Color CustomLed { get; set; } = new(0, 0, 255);

        public DS4Color Led { get; set; }

        public DS4Color LowLed { get; set; }

        public DS4Color ChargingLed { get; set; }

        public DS4Color FlashLed { get; set; }

        public double Rainbow { get; set; }

        public double MaxRainbowSaturation { get; set; } = 1.0;

        public int FlashAt { get; set; } // Battery % when flashing occurs. <0 means disabled

        public byte FlashType { get; set; }

        public int ChargingType { get; set; }
    }

    public class LightbarSettingInfo
    {
        private LightbarMode mode = LightbarMode.DS4Win;

        public LightbarDS4WinInfo Ds4WinSettings { get; } = new();

        public LightbarMode Mode
        {
            get => mode;
            set
            {
                if (mode == value) return;
                mode = value;
                ModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ModeChanged;
    }

    public class SteeringWheelSmoothingInfo
    {
        private double minCutoff = OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF;
        private double beta = OneEuroFilterPair.DEFAULT_WHEEL_BETA;
        public bool enabled = false;

        public delegate void SmoothingInfoEventHandler(SteeringWheelSmoothingInfo sender, EventArgs args);

        public double MinCutoff
        {
            get => minCutoff;
            set
            {
                if (minCutoff == value) return;
                minCutoff = value;
                MinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event SmoothingInfoEventHandler MinCutoffChanged;

        public double Beta
        {
            get => beta;
            set
            {
                if (beta == value) return;
                beta = value;
                BetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event SmoothingInfoEventHandler BetaChanged;

        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public void Reset()
        {
            MinCutoff = OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF;
            Beta = OneEuroFilterPair.DEFAULT_WHEEL_BETA;
            enabled = false;
        }

        public void SetFilterAttrs(OneEuroFilter euroFilter)
        {
            euroFilter.MinCutoff = minCutoff;
            euroFilter.Beta = beta;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) =>
            {
                euroFilter.Beta = beta;
            };

            MinCutoffChanged += (sender, args) =>
            {
                euroFilter.MinCutoff = minCutoff;
            };
        }
    }


    public class TouchpadRelMouseSettings
    {
        public const double DEFAULT_ANG_DEGREE = 0.0;
        public const double DEFAULT_ANG_RAD = DEFAULT_ANG_DEGREE * Math.PI / 180.0;
        public const double DEFAULT_MIN_THRESHOLD = 1.0;

        public double rotation = DEFAULT_ANG_RAD;
        public double minThreshold = DEFAULT_MIN_THRESHOLD;

        public void Reset()
        {
            rotation = DEFAULT_ANG_RAD;
            minThreshold = DEFAULT_MIN_THRESHOLD;
        }
    }

    public class TouchpadAbsMouseSettings
    {
        public const int DEFAULT_MAXZONE_X = 90;
        public const int DEFAULT_MAXZONE_Y = 90;
        public const bool DEFAULT_SNAP_CENTER = false;

        public int maxZoneX = DEFAULT_MAXZONE_X;
        public int maxZoneY = DEFAULT_MAXZONE_Y;
        public bool snapToCenter = DEFAULT_SNAP_CENTER;

        public void Reset()
        {
            maxZoneX = DEFAULT_MAXZONE_X;
            maxZoneY = DEFAULT_MAXZONE_Y;
            snapToCenter = DEFAULT_SNAP_CENTER;
        }
    }

    public enum StickMode : uint
    {
        None,
        Controls,
        FlickStick,
    }

    public enum TriggerMode : uint
    {
        Normal,
        TwoStage,
    }

    public enum TwoStageTriggerMode : uint
    {
        Disabled,
        Normal,
        ExclusiveButtons,
        HairTrigger,
        HipFire,
        HipFireExclusiveButtons,
    }

    public class FlickStickSettings
    {
        public const double DEFAULT_FLICK_THRESHOLD = 0.9;
        public const double DEFAULT_FLICK_TIME = 0.1;  // In seconds
        public const double DEFAULT_REAL_WORLD_CALIBRATION = 5.3;
        public const double DEFAULT_MIN_ANGLE_THRESHOLD = 0.0;

        public const double DEFAULT_MINCUTOFF = 0.4;
        public const double DEFAULT_BETA = 0.4;

        public double flickThreshold = DEFAULT_FLICK_THRESHOLD;
        public double flickTime = DEFAULT_FLICK_TIME; // In seconds
        public double realWorldCalibration = DEFAULT_REAL_WORLD_CALIBRATION;
        public double minAngleThreshold = DEFAULT_MIN_ANGLE_THRESHOLD;

        public double minCutoff = DEFAULT_MINCUTOFF;
        public double beta = DEFAULT_BETA;

        public delegate void FlickStickSettingsEventHandler(FlickStickSettings sender,
           EventArgs args);

        public double MinCutoff
        {
            get => minCutoff;
            set
            {
                if (minCutoff == value) return;
                minCutoff = value;
                MinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event FlickStickSettingsEventHandler MinCutoffChanged;

        public double Beta
        {
            get => beta;
            set
            {
                if (beta == value) return;
                beta = value;
                BetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event FlickStickSettingsEventHandler BetaChanged;

        public void Reset()
        {
            flickThreshold = DEFAULT_FLICK_THRESHOLD;
            flickTime = DEFAULT_FLICK_TIME;
            realWorldCalibration = DEFAULT_REAL_WORLD_CALIBRATION;
            minAngleThreshold = DEFAULT_MIN_ANGLE_THRESHOLD;

            minCutoff = DEFAULT_MINCUTOFF;
            beta = DEFAULT_BETA;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) =>
            {
                euroFilter.Beta = beta;
            };

            MinCutoffChanged += (sender, args) =>
            {
                euroFilter.MinCutoff = minCutoff;
            };
        }

        public void RemoveRefreshEvents()
        {
            BetaChanged = null;
            MinCutoffChanged = null;
        }
    }

    public class StickControlSettings
    {
        public void Reset()
        {
        }
    }

    public class StickModeSettings
    {
        public FlickStickSettings flickSettings = new FlickStickSettings();
        public StickControlSettings controlSettings = new StickControlSettings();
    }

    public class StickOutputSetting
    {
        public StickMode mode = StickMode.Controls;
        public StickModeSettings outputSettings = new StickModeSettings();

        public void ResetSettings()
        {
            mode = StickMode.Controls;
            outputSettings.controlSettings.Reset();
            outputSettings.flickSettings.Reset();
        }
    }

    public class TriggerOutputSettings
    {
        private const TwoStageTriggerMode DEFAULT_TRIG_MODE = TwoStageTriggerMode.Disabled;
        private const int DEFAULT_HIP_TIME = 100;
        private const InputDevices.TriggerEffects DEFAULT_TRIGGER_EFFECT = InputDevices.TriggerEffects.None;

        //public TriggerMode mode = TriggerMode.Normal;
        public TwoStageTriggerMode twoStageMode = DEFAULT_TRIG_MODE;
        public TwoStageTriggerMode TwoStageMode
        {
            get => twoStageMode;
            set
            {
                if (twoStageMode == value) return;
                twoStageMode = value;
                TwoStageModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TwoStageModeChanged;

        public int hipFireMS = DEFAULT_HIP_TIME;
        public InputDevices.TriggerEffects triggerEffect = DEFAULT_TRIGGER_EFFECT;
        public InputDevices.TriggerEffects TriggerEffect
        {
            get => triggerEffect;
            set
            {
                if (triggerEffect == value) return;
                triggerEffect = value;
                TriggerEffectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TriggerEffectChanged;

        public InputDevices.TriggerEffectSettings effectSettings =
            new InputDevices.TriggerEffectSettings();
        public ref InputDevices.TriggerEffectSettings TrigEffectSettings
        {
            get => ref effectSettings;
        }

        public void ResetSettings()
        {
            //mode = TriggerMode.Normal;
            twoStageMode = DEFAULT_TRIG_MODE;
            hipFireMS = DEFAULT_HIP_TIME;
            triggerEffect = DEFAULT_TRIGGER_EFFECT;
            TwoStageModeChanged?.Invoke(this, EventArgs.Empty);
            TriggerEffectChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ResetEvents()
        {
            TwoStageModeChanged = null;
            TriggerEffectChanged = null;
        }
    }
}