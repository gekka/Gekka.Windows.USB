//MIT License

//Copyright (c) 2022 gekka

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

namespace Gekka.Windows.USB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::Windows.Win32.Devices.HumanInterfaceDevice;
    using global::Windows.Win32.UI.Input;
    using static global::Windows.Win32.PInvoke;

    namespace Input.HID
    {
        using System.Collections.ObjectModel;

        public sealed class HIDInputData : InputData
        {
            internal HIDInputData(RAWINPUT rawInput, byte[] rawReport = null) : base(rawInput)
            {
                this.rawReport = rawReport;
            }

            private byte[] rawReport;
            private byte[] preparsedData;

            public bool HasCaps { get; private set; }

            private HIDP_CAPS _Caps;

            public IReadOnlyCollection<IButtonItem> InputButtons => _InputButtonItems;
            private ReadOnlyCollection<ButtonItem> _InputButtonItems;

            public IReadOnlyCollection<IValueItem> InputValues => _InputValueItems;
            private ReadOnlyCollection<ValueItem> _InputValueItems;

            //public IReadOnlyCollection<IButtonItem> FeatureButtons => _FeatureButtonItems;
            //private ReadOnlyCollection<ButtonItem> _FeatureButtonItems;

            //public IReadOnlyCollection<IValueItem> FeatureValues => _FeatureValueItems;
            //private ReadOnlyCollection<ValueItem> _FeatureValueItems;
            #region

            public override bool GetCaps()
            {
                base.GetCaps();

                if (!GetRawInputDeviceInfo<byte[]>(out var preparsedData, this.RawInput, RAW_INPUT_DEVICE_INFO_COMMAND.RIDI_PREPARSEDDATA))
                {
                    return false;
                }

                if (!GetCaps(preparsedData, out var caps))
                {
                    return false;
                }

                if (!GetInputButtonCaps(out var bicaps, preparsedData, caps))
                {
                    return false;
                }

                if (!GetInputValueCaps(out var vicaps, preparsedData, caps))
                {
                    return false;
                }

                //if (!GetOutputButtonCaps(out var bocaps, preparsedData, caps))
                //{
                //    return false;
                //}
                //if (!GetOutputValueCaps(out var vocaps, preparsedData, caps))
                //{
                //    return false;
                //}

                //if (!GetFeatureButtonCaps(out var bfcaps, preparsedData, caps))
                //{
                //    return false;
                //}

                //if (!GetFeatureValueCaps(out var vfcaps, preparsedData, caps))
                //{
                //    return false;
                //}


                this._Caps = caps;
                this.HasCaps = true;
                this.preparsedData = preparsedData;

                this._InputButtonItems = Array.AsReadOnly(bicaps.Select(_ => new ButtonItem(_)).ToArray());
                this._InputValueItems = Array.AsReadOnly(vicaps.Select(_ => new ValueItem(_)).ToArray());

                //this._FeatureButtonItems = Array.AsReadOnly(bfcaps.Select(_ => new ButtonItem(_)).ToArray());
                //this._FeatureValueItems = Array.AsReadOnly(vfcaps.Select(_ => new ValueItem(_)).ToArray());

                var ccaps = _InputValueItems
                                .OfType<ItemBase>()
                                .Union(_InputButtonItems)
                                .OrderBy(_ => _.DataIndexMin).ToArray();

                int bitpos = 8; //レポート番号スキップ
                foreach (ItemBase c in ccaps)
                {
                    c.SetBitPos(ref bitpos);//生データのビット位置を設定
                }
                
                return true;
            }


            private static bool GetCaps(in byte[] preparsedData, out HIDP_CAPS caps)
            {
                unsafe
                {
                    fixed (byte* bp = preparsedData)
                    {

                        IntPtr ip = new IntPtr(bp);
                        return HidP_GetCaps(ip, out caps).Value == (uint)HIDP_STATUS.HIDP_STATUS_SUCCESS;
                    }
                }
            }

            private static bool GetInputButtonCaps(out HIDP_BUTTON_CAPS[] bcaps, in byte[] preparsedData, in HIDP_CAPS caps)
            {
                return GetButtonCaps(out bcaps, preparsedData, HIDP_REPORT_TYPE.HidP_Input, caps.NumberInputButtonCaps);
            }
            private static bool GetOutputButtonCaps(out HIDP_BUTTON_CAPS[] bcaps, in byte[] preparsedData, in HIDP_CAPS caps)
            {
                return GetButtonCaps(out bcaps, preparsedData, HIDP_REPORT_TYPE.HidP_Output, caps.NumberOutputButtonCaps);
            }
            private static bool GetFeatureButtonCaps(out HIDP_BUTTON_CAPS[] bcaps, in byte[] preparsedData, in HIDP_CAPS caps)
            {
                return GetButtonCaps(out bcaps, preparsedData, HIDP_REPORT_TYPE.HidP_Feature, caps.NumberFeatureButtonCaps);
            }

            private static bool GetButtonCaps(out HIDP_BUTTON_CAPS[] bcaps, in byte[] preparsedData, HIDP_REPORT_TYPE reportType, ushort length)
            {
                bcaps = new HIDP_BUTTON_CAPS[length];
                if (length == 0)
                {
                    return true;
                }

                unsafe
                {
                    fixed (byte* pb = preparsedData)
                    {
                        fixed (HIDP_BUTTON_CAPS* p = bcaps)
                        {
                            var status = (HIDP_STATUS)HidP_GetButtonCaps(reportType, p, ref length, new IntPtr(pb)).Value;
                            return status == HIDP_STATUS.HIDP_STATUS_SUCCESS;
                        }
                    }
                }
            }

            private static bool GetInputValueCaps(out HIDP_VALUE_CAPS[] vcaps, in byte[] preparsedData, in HIDP_CAPS caps)
            {
                return GetValueCaps(out vcaps, preparsedData, HIDP_REPORT_TYPE.HidP_Input, caps.NumberInputValueCaps);
            }
            private static bool GetOutputValueCaps(out HIDP_VALUE_CAPS[] vcaps, in byte[] preparsedData, in HIDP_CAPS caps)
            {
                return GetValueCaps(out vcaps, preparsedData, HIDP_REPORT_TYPE.HidP_Output, caps.NumberOutputValueCaps);
            }
            private static bool GetFeatureValueCaps(out HIDP_VALUE_CAPS[] vcaps, in byte[] preparsedData, in HIDP_CAPS caps)
            {
                return GetValueCaps(out vcaps, preparsedData, HIDP_REPORT_TYPE.HidP_Feature, caps.NumberFeatureValueCaps);
            }
            private static bool GetValueCaps(out HIDP_VALUE_CAPS[] vcaps, in byte[] preparsedData, HIDP_REPORT_TYPE reportType, ushort length)
            {
                vcaps = new HIDP_VALUE_CAPS[length];
                if (length == 0)
                {
                    return true;
                }
                unsafe
                {
                    fixed (byte* pb = preparsedData)
                    fixed (HIDP_VALUE_CAPS* p = vcaps)
                    {
                        var status = (HIDP_STATUS)HidP_GetValueCaps(reportType, p, ref length, new IntPtr(pb)).Value;
                        return status == HIDP_STATUS.HIDP_STATUS_SUCCESS;
                    }
                }
            }

            #endregion
            internal void Parse(HIDInputData rawReportSource)
            {
                this.rawReport = rawReportSource.rawReport;
                Parse();
            }

            public void Parse()
            {
#if true
                var bitArray = new System.Collections.BitArray(rawReport);
                foreach (ButtonItem cbcap in _InputButtonItems)
                {
                    cbcap.Get(bitArray);
                }

                foreach (ValueItem cvcap in _InputValueItems)
                {
                    cvcap.Get(bitArray);
                }
#else
                unsafe
                {
                    uint reportLength = (uint)this.rawReport.Length;
                    fixed (byte* pReport = this.rawReport)
                    fixed (byte* pPreData = preparsedData)
                    {
                        global::Windows.Win32.Foundation.PSTR pstrReport = new global::Windows.Win32.Foundation.PSTR(pReport);
                        IntPtr ipPreData = new IntPtr(pPreData);
                        foreach (ButtonItem cbcap in _InputButtonItems)
                        {
                            cbcap.Get(ipPreData, pstrReport, reportLength);
                        }
                        foreach (ValueItem cvcap in _InputValueItems)
                        {
                            cvcap.Get(ipPreData, pstrReport, reportLength);
                        }
                    }
                }

#endif

#if DEBUG_DUMP
                System.Diagnostics.Debug.WriteLine(string.Join(",", rawReport.Select(_ =>
                {
                    string s = "";

                    for (int i = 0; i < 8; i++)
                    {
                        if ((_ & 0x01) == 0x1)
                        {
                            s += "1";
                        }
                        else
                        {
                            s += "0";
                        }
                        _ >>= 1;
                    }
                    return s;
                })));
#endif
            }
        }
    }
}