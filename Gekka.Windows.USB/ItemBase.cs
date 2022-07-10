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

    using global::Windows.Win32;
    using global::Windows.Win32.Devices.HumanInterfaceDevice;
    using global::Windows.Win32.Foundation;
    using static global::Windows.Win32.PInvoke;

    namespace Input.HID
    {
        abstract class ItemBase : Common.ModelBase
        {
            internal ItemBase(IHIDP_CAPSBase caps)
            {
                this.ICaps = caps;
                this.ReportID = caps.ReportID;
                this.IsRange = caps.IsRange.Value != 0;
            }
            internal IHIDP_CAPSBase ICaps { get; }

            internal byte ReportID { get; }
            internal bool IsRange { get; private set; }
            internal int DataIndexMin { get; private protected set; } = -1;
            internal int DataIndexMax { get; private protected set; } = -1;
            internal int DataSize => (DataIndexMax - DataIndexMin + 1);

            private protected int _Bitpos = -1;

            internal abstract void SetBitPos(ref int bitpos);
            internal abstract void Get(System.Collections.BitArray ba);
            internal abstract void Get(IntPtr ippre, PSTR pstrReport, uint reportLength);
        }

        abstract class ItemBase<T, U> : ItemBase where T : IHIDP_CAPSBase
        {
            internal ItemBase(T caps) : base(caps)
            {
                this.Caps = caps;
                SetDataSize(caps);

                _ValuesArray = new Common.SimpleModel<U>[this.DataSize];
                for (int i = 0; i < this.DataSize; i++)
                {
                    _ValuesArray[i] = new Common.SimpleModel<U>();
                }
                Values = System.Array.AsReadOnly(_ValuesArray);
            }

            protected abstract void SetDataSize(T caps);

            internal T Caps { get; }

            protected Common.SimpleModel<U>[] _ValuesArray;

            public IReadOnlyCollection<Common.SimpleModel<U>> Values { get; }
        }

        /// <summary>HIDの値の情報</summary>
        class ValueItem : ItemBase<HIDP_VALUE_CAPS, ushort>, IValueItem
        {
            internal ValueItem(HIDP_VALUE_CAPS caps) : base(caps)
            {
                if (caps.UsagePage == (ushort)Input.HID.HID_UsagePage.GenericDesktopPage && !this.IsRange)
                {
                    Usage = ((Input.HID.HID_GenericDesktopPage_Usage)caps.Anonymous.NotRange.Usage).ToString();
                }
            }

            protected override void SetDataSize(HIDP_VALUE_CAPS caps)
            {
                if (this.IsRange)
                {
                    this.DataIndexMin = caps.Anonymous.Range.DataIndexMin;
                    this.DataIndexMax = caps.Anonymous.Range.DataIndexMax;
                }
                else
                {
                    this.DataIndexMax = this.DataIndexMin = caps.Anonymous.NotRange.DataIndex;
                }
            }

            public string Usage { get; }

            internal override void SetBitPos(ref int bitpos)
            {
                this._Bitpos = bitpos;
                bitpos += this.Caps.BitSize * DataSize;
            }

            /// <summary>APIを使わずに値の取り出し</summary>
            /// <param name="ba"></param>
            internal override void Get(System.Collections.BitArray ba)
            {
                if (_Bitpos < 0)
                {
                    return;
                }

                int p = _Bitpos;
                int len = this.Caps.BitSize;

                int size = this.DataSize;
                for (int index = 0; index < size; index++)
                {
                    ushort us = 0;
                    ushort f = 1;
                    for (int i = 0; i < len; i++)
                    {
                        if (ba[p++])
                        {
                            us |= f;
                        }
                        f <<= 1;
                    }

                    _ValuesArray[index].Value = us;

                }
            }


            /// <summary>APIを使って値の取り出し</summary>
            /// <param name="ippre"></param>
            /// <param name="pstrReport"></param>
            /// <param name="reportLength"></param>
            internal override void Get(IntPtr ippre, PSTR pstrReport, uint reportLength)
            {
                ReadValues(new[] { this }, ippre, pstrReport, reportLength);
            }

            private static unsafe void ReadValues(IEnumerable<ValueItem> vis, IntPtr ippre, PSTR pstrReport, uint reportLength)
            {
                foreach (ValueItem item in vis)
                {
                    if (item._Bitpos < 0)
                    {
                        continue;
                    }
                    if (item.Caps.UsagePage >= (ushort)HID_UsagePage.VenderDefined_Start)
                    {
                        continue;
                    }

                    if (item.IsRange)
                    {

                    }
                    else
                    {
                        uint uv = 0;
                        ushort usage = item.Caps.Anonymous.NotRange.Usage;
                        HIDP_STATUS status;

                        status = (HIDP_STATUS)HidP_GetUsageValue(HIDP_REPORT_TYPE.HidP_Input, item.Caps.UsagePage, (ushort)item.Caps.LinkCollection, usage, out uv, ippre, pstrReport, reportLength).Value;
                        if (status == HIDP_STATUS.HIDP_STATUS_SUCCESS)
                        {
                            item._ValuesArray[0].Value = checked((ushort)uv);


                            //if (cvcap.Caps.UsagePage == (ushort)HID_UsagePage.GenericDesktopPage)
                            //{
                            //    var gdpUsage = (HID_GenericDesktopPage_Usage)usage;
                            //}
                        }
                    }

                }
            }
        }

        /// <summary>HIDのボタンの情報</summary>
        class ButtonItem : ItemBase<HIDP_BUTTON_CAPS, bool>, IButtonItem
        {
            internal ButtonItem(HIDP_BUTTON_CAPS caps) : base(caps)
            {
            }
            protected override void SetDataSize(HIDP_BUTTON_CAPS caps)
            {
                if (caps.IsRange.Value != 0)
                {
                    this.DataIndexMin = caps.Anonymous.Range.DataIndexMin;
                    this.DataIndexMax = caps.Anonymous.Range.DataIndexMax;
                }
                else
                {
                    this.DataIndexMax = this.DataIndexMin = caps.Anonymous.NotRange.DataIndex;
                }
            }

            internal override void SetBitPos(ref int bitpos)
            {
                this._Bitpos = bitpos;
                bitpos += this.DataSize;
            }

            /// <summary>APIを使わずボタンの値を取り出す</summary>
            internal override void Get(System.Collections.BitArray ba)
            {
                if (_Bitpos < 0)
                {
                    return;
                }

                int p = _Bitpos;
                int index = 0;
                int size = this.DataSize;

                while (index < size)
                {
                    _ValuesArray[index++].Value = ba[p++];
                }
            }

            /// <summary>APIを使ってボタンの値を取り出す</summary>
            /// <param name="ippre"></param>
            /// <param name="pstrReport"></param>
            /// <param name="reportLength"></param>
            internal override void Get(IntPtr ippre, PSTR pstrReport, uint reportLength)
            {
                ReadButtons(new[] { this }, ippre, pstrReport, reportLength);
            }

            private static unsafe void ReadButtons(IEnumerable<ButtonItem> bis, IntPtr ipPreData, PSTR pstrReport, uint reportLength)
            {
                foreach (ButtonItem item in bis)
                {
                    if (item._Bitpos < 0)
                    {
                        continue;
                    }

                    uint size = (uint)item.DataSize;
                    HIDP_STATUS status = HIDP_STATUS.HIDP_STATUS_NULL;
                    ushort[] usageList = new ushort[0];
                    for (int i = 0; i < 2; i++)
                    {
                        usageList = new ushort[size];

                        fixed (ushort* plist = usageList)
                        {
                            status = (HIDP_STATUS)PInvoke.HidP_GetUsages(HIDP_REPORT_TYPE.HidP_Input, item.Caps.UsagePage, item.Caps.LinkCollection, plist, ref size, ipPreData, pstrReport, reportLength).Value;
                        }

                        if (status == HIDP_STATUS.HIDP_STATUS_BUFFER_TOO_SMALL)
                        {
                            continue;
                        }
                        break;
                    }

                    if (status == HIDP_STATUS.HIDP_STATUS_SUCCESS)
                    {
                        if (item.IsRange)
                        {
                            for (int i = 0; i < size; i++)
                            {
                                item._ValuesArray[i].Value = (usageList[i] != 0);
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }


        }


        /// <summary>HIDの値の公開用インターフェース</summary>
        public interface IValueItem
        {
            /// <summary>値の種類</summary>
            string Usage { get; }

            /// <summary>値の状態の一覧</summary>
            IReadOnlyCollection<Common.SimpleModel<ushort>> Values { get; }
        }

        /// <summary>HIDのボタンの公開用インターフェース</summary>
        public interface IButtonItem
        {
            /// <summary>ボタンの状態一覧</summary>
            IReadOnlyCollection<Common.SimpleModel<bool>> Values { get; }
        }
    }


}