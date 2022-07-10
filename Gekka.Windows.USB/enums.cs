

namespace Gekka.Windows.USB
{
    namespace Input
    {
        enum RIM_TYPE : uint
        {
            MOUSE = 0,
            KEYBOARD = 1,
            HID = 2,
        }
    }

    namespace Input.HID
    {
        internal enum HIDP_STATUS : uint
        {
            HIDP_STATUS_SUCCESS = (uint)((0x0 << 28) | (0x11 << 16) | (0)),
            HIDP_STATUS_NULL = unchecked((0x8u << 28) | (0x11u << 16) | (1)),
            HIDP_STATUS_INVALID_PREPARSED_DATA = unchecked((0x0Cu << 28) | (0x11u << 16) | (0x01)),
            HIDP_STATUS_INVALID_REPORT_TYPE = unchecked((0x0Cu << 28) | (0x11u << 16) | (0x2)),
            HIDP_STATUS_INVALID_REPORT_LENGTH = unchecked((0x0Cu << 28) | (0x11u << 16) | (0x3)),
            HIDP_STATUS_USAGE_NOT_FOUND = unchecked((0x0Cu << 28) | (0x11u << 16) | (0x4)),
            HIDP_STATUS_VALUE_OUT_OF_RANGE = unchecked((0x0Cu << 28) | (0x11u << 16) | (0x5)),
            HIDP_STATUS_BAD_LOG_PHY_VALUES = unchecked((0x0Cu << 28) | (0x11u << 16) | (0x6)),
            HIDP_STATUS_BUFFER_TOO_SMALL = unchecked((0x0Cu << 28) | (0x11u << 16) | (0x7)),
            HIDP_STATUS_INTERNAL_ERROR = unchecked((0x0Cu << 28) | (0x11u << 16) | (0x8)),
            HIDP_STATUS_I8042_TRANS_UNKNOWN = unchecked((0x0Cu << 28) | (0x11u << 16) | (0x9)),
            HIDP_STATUS_INCOMPATIBLE_REPORT_ID = unchecked((0x0Cu << 28) | (0x11u << 16) | (0x0A)),
            HIDP_STATUS_NOT_VALUE_ARRAY = unchecked((0x0Cu << 28) | (0x11u << 16) | (0x0B)),
            HIDP_STATUS_IS_VALUE_ARRAY = unchecked((0x0Cu << 28) | (0x11 << 16) | (0x0C)),
            HIDP_STATUS_DATA_INDEX_NOT_FOUND = unchecked((0x0Cu << 28) | (0x11 << 16) | (0x0D)),
            HIDP_STATUS_DATA_INDEX_OUT_OF_RANGE = unchecked((0x0Cu << 28) | (0x11 << 16) | (0x0E)),
            HIDP_STATUS_BUTTON_NOT_PRESSED = unchecked((0x0Cu << 28) | (0x11 << 16) | (0x0F)),
            HIDP_STATUS_REPORT_DOES_NOT_EXIST = unchecked((0x0Cu << 28) | (0x11 << 16) | (0x10)),
            HIDP_STATUS_NOT_IMPLEMENTED = unchecked((0x0Cu << 28) | (0x11 << 16) | (0x20)),

        }

        public enum HID_UsagePage : ushort
        {
            GenericDesktopPage = 0x01,

            SimulationControlsPage = 0x02,
            VRControlsPage = 0x03,
            SportControlsPage = 0x04,
            GameControlsPage = 0x05,
            GenericDeviceControlsPage = 0x06,
            Keyboard_KeypadPage = 0x07,
            LEDPage = 0x08,

            ButtonPage = 0x09,

            OrdinalPage = 0x0A,
            TelephonyDevicePage = 0x0B,
            ConsumerPage = 0x0C,
            DigitizersPage = 0x0D,
            HapticsPage = 0x0E,
            PIDPage = 0x0F,
            UnicodePage = 0x10,
            EyeAndHeadTrackersPage = 0x12,
            AuxiliaryDisplayPage = 0x14,
            SensorsPage = 0x20,
            MedicalInstrumentPage = 0x40,
            BrailleDisplayPage = 0x41,
            LightingAndIlluminationPage = 0x59,

            MonitorPage_0 = 0x80,
            MonitorPage_1 = 0x81,
            MonitorPage_2 = 0x82,
            MonitorPage_3 = 0x83,

            PowerPages_0 = 0x84,
            PowerPages_1 = 0x85,
            PowerPages_2 = 0x86,
            PowerPages_3 = 0x87,

            BarCodeScannerPage = 0x8C,
            ScalePage = 0x8D,
            MagneticStripeReadingDevices = 0x8E,
            ReservedPointOfSalePages = 0x8F,
            CameraControlPage = 0x90,
            ArcadePage = 0x92,
            GamingDevicePage = 0x92,
            FIDOAliancePge = 0xF1D0,

            VenderDefined_Start = 0xFF00,
        }

        public enum HID_GenericDesktop_ApplicationCollection : ushort
        {
            Pointer = 0x01,

            Mouse = 0x02,

            Joystick = 0x04,
            Gamepad = 0x05,

            Keyboard = 0x06,
            Keypad = 0x07,
            MultiAxisController0x08,
            TabletPCSystemControls = 0x09,
            WaterCoolingDevice = 0x0A,
            ComputerChassisDevice = 0x0B,
            WirelessRadioControls = 0x0C,
            PortableDeviceControl = 0x0D,
            SystemMultiAxisController = 0x0E,
            SpatialController = 0x0F,
            AssistiveControl = 0x10,
            DeviceDock = 0x11,
            DockableDevice = 0x12,
        }

        public enum HID_GenericDesktopPage_Usage : ushort
        {
            X = 0x30,
            Y = 0x31,
            Z = 0x32,
            Rx = 0x33,
            Ry = 0x34,
            Rz = 0x35,
            Slider = 0x36,
            Dial = 0x37,
            Wheel = 0x38,
            HatSwitch = 0x39,
            CountedBuffer = 0x3A,
            ByteCount = 0x3B,
            MotionWakeup = 0x3C,
            Start = 0x3D,
            Select = 0x3E,
            Vx = 0x40,
            Vy = 0x41,
            Vz = 0x42,
            Vbrx = 0x43,

            Vbry = 0x44,
            Vbrz = 0x45,
            Vno = 0x46,
            FeatureNotification = 0x47,
            ResolutionMultiplier0x48,
            Qx = 0x49,
            Qy = 0x4A,
            Qz = 0x4B,
            Qw = 0x4C,
        }

    }
}