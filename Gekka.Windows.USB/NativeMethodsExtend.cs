namespace Windows.Win32
{
    using global::System;
    using global::System.Diagnostics;
    using global::System.Runtime.CompilerServices;
    using global::System.Runtime.InteropServices;
    using Windows.Win32.Foundation;
    using winmdroot = global::Windows.Win32;

    namespace Devices.HumanInterfaceDevice
    {

        internal interface IHIDP_CAPSBase
        {
            internal ushort UsagePage { get; }
            internal byte ReportID { get; }
            internal winmdroot.Foundation.BOOLEAN IsAlias { get; }
            internal ushort BitField { get; }
            internal ushort LinkCollection { get; }
            internal ushort LinkUsage { get; }
            internal ushort LinkUsagePage { get; }
            internal winmdroot.Foundation.BOOLEAN IsRange { get; }
            internal winmdroot.Foundation.BOOLEAN IsStringRange { get; }
            internal winmdroot.Foundation.BOOLEAN IsDesignatorRange { get; }
            internal winmdroot.Foundation.BOOLEAN IsAbsolute { get; }
        }

        internal partial struct HIDP_VALUE_CAPS : IHIDP_CAPSBase
        {
            ushort IHIDP_CAPSBase.UsagePage { get => UsagePage; }
            byte IHIDP_CAPSBase.ReportID { get => ReportID; }
            BOOLEAN IHIDP_CAPSBase.IsAlias { get => IsAlias; }
            ushort IHIDP_CAPSBase.BitField { get => BitField; }
            ushort IHIDP_CAPSBase.LinkCollection { get => LinkCollection; }
            ushort IHIDP_CAPSBase.LinkUsage { get => LinkUsage; }
            ushort IHIDP_CAPSBase.LinkUsagePage { get => LinkUsage; }
            BOOLEAN IHIDP_CAPSBase.IsRange { get => IsRange; }
            BOOLEAN IHIDP_CAPSBase.IsStringRange { get => IsStringRange; }
            BOOLEAN IHIDP_CAPSBase.IsDesignatorRange { get => IsDesignatorRange; }
            BOOLEAN IHIDP_CAPSBase.IsAbsolute { get => IsAbsolute; }
        }
        internal partial struct HIDP_BUTTON_CAPS : IHIDP_CAPSBase
        {
            ushort IHIDP_CAPSBase.UsagePage { get => UsagePage; }
            byte IHIDP_CAPSBase.ReportID { get => ReportID; }
            BOOLEAN IHIDP_CAPSBase.IsAlias { get => IsAlias; }
            ushort IHIDP_CAPSBase.BitField { get => BitField; }
            ushort IHIDP_CAPSBase.LinkCollection { get => LinkCollection; }
            ushort IHIDP_CAPSBase.LinkUsage { get => LinkUsage; }
            ushort IHIDP_CAPSBase.LinkUsagePage { get => LinkUsage; }
            BOOLEAN IHIDP_CAPSBase.IsRange { get => IsRange; }
            BOOLEAN IHIDP_CAPSBase.IsStringRange { get => IsStringRange; }
            BOOLEAN IHIDP_CAPSBase.IsDesignatorRange { get => IsDesignatorRange; }
            BOOLEAN IHIDP_CAPSBase.IsAbsolute { get => IsAbsolute; }
        }
    }
}