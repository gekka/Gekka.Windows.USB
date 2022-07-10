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

    using global::Windows.Win32;
    using global::Windows.Win32.UI.Input;

    using static System.Runtime.InteropServices.Marshal;


    namespace Input
    {
        using Input.HID;

        /// <summary>デバイス情報</summary>
        public class InputData
        {
            /// <summary>デバイス情報を取得</summary>
            /// <param name="msg_lparam"></param>
            /// <param name="inputData">HIDではない場合</param>
            /// <param name="hidInputData">HIDの場合の</param>
            /// <returns></returns>
            internal static bool GetRawInputData(IntPtr msg_lparam, out InputData inputData, out HIDInputData hidInputData)
            {
                inputData = null;
                hidInputData = null;

                HRAWINPUT hRawInput = new HRAWINPUT(msg_lparam);

                uint sizeHeader = (uint)SizeOf<RAWINPUTHEADER>();
                uint sizeRawInput = 0; ;

                unsafe
                {
                    void* p = IntPtr.Zero.ToPointer();
                    uint ret = PInvoke.GetRawInputData(hRawInput, RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT, p, ref sizeRawInput, sizeHeader);
                    if (ret != 0)
                    {
                        return false;
                    }

                    IntPtr ip = AllocCoTaskMem((int)sizeRawInput);
                    try
                    {
                        p = ip.ToPointer();

                        uint byteCount = PInvoke.GetRawInputData(hRawInput, RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT, p, ref sizeRawInput, sizeHeader);
                        if (byteCount < sizeRawInput)
                        {
                            return false;
                        }

                        RAWINPUT rawInput = PtrToStructure<global::Windows.Win32.UI.Input.RAWINPUT>(ip);

                        if (rawInput.header.dwType == (uint)RIM_TYPE.HID)
                        {
                            uint dwSizeHid = rawInput.data.hid.dwSizeHid;

                            byte* pb = (byte*)p + sizeof(RAWINPUTHEADER) + (sizeof(RAWHID) - sizeof(byte*));///* sizeof(rawInput.data.hid.bRawData) */);

                            byte[] rawReport = new byte[rawInput.data.hid.dwSizeHid * rawInput.data.hid.dwCount];
                            fixed (byte* pbhid = rawReport)
                            {
                                System.Runtime.CompilerServices.Unsafe.CopyBlock(pbhid, pb, (uint)rawReport.Length);
                            }
                            hidInputData = new HIDInputData(rawInput, rawReport);
                            inputData = hidInputData;
                        }
                        else
                        {
                            inputData = new InputData(rawInput);
                        }

                        return true;
                    }
                    finally
                    {
                        System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ip);
                    }
                }
            }

            internal InputData(RAWINPUT rawInput)
            {
                this.RawInput = rawInput;
            }

            internal global::Windows.Win32.UI.Input.RAWINPUT RawInput { get; }

            public string DeviceName { get; internal set; }

            public bool IsMouse => RawInput.header.dwType == (uint)RIM_TYPE.MOUSE;
            public bool IsKeyborad => RawInput.header.dwType == (uint)RIM_TYPE.KEYBOARD;
            public bool IsHID => RawInput.header.dwType == (uint)RIM_TYPE.HID;

            /// <summary>通信解析用設定を取得</summary>
            /// <returns></returns>
            public virtual bool GetCaps()
            {
                if (!GetRawInputDeviceInfo<byte[]>(out var namebyte, this.RawInput, RAW_INPUT_DEVICE_INFO_COMMAND.RIDI_DEVICENAME))
                {
                    return false;
                }

                this.DeviceName = System.Text.Encoding.Unicode.GetString(namebyte).TrimEnd('\0');

                //if (!GetRawInputDeviceInfo<RID_DEVICE_INFO>(out var ridDevInfo, this.RawInput, RAW_INPUT_DEVICE_INFO_COMMAND.RIDI_DEVICEINFO))
                //{
                //    return false;
                //}

                return true;
            }

            private protected static bool GetRawInputDeviceInfo<T>(out T result, global::Windows.Win32.UI.Input.RAWINPUT rawInput, RAW_INPUT_DEVICE_INFO_COMMAND command)
            {
                unsafe
                {
                    result = default(T);

                    Common.SafeHandle sf = new Common.SafeHandle(rawInput.header.hDevice, false);

                    uint sizePre = 0;
                    void* p = IntPtr.Zero.ToPointer();
                    if (uint.MaxValue == PInvoke.GetRawInputDeviceInfo(sf, command, p, ref sizePre))
                    {
                        return false;
                    }
                    if (sizePre == 0)
                    {
                        return false;
                    }

                    if (command == RAW_INPUT_DEVICE_INFO_COMMAND.RIDI_DEVICENAME)
                    {
                        sizePre *= 2;
                    }

                    byte[] bs = new byte[sizePre];
                    fixed (void* pb = bs)
                    {
                        PInvoke.GetRawInputDeviceInfo(sf, command, pb, ref sizePre);

                        if (typeof(T) == typeof(byte[]))
                        {
                            result = (T)(object)bs;
                        }
                        else
                        {
                            result = (T)System.Runtime.InteropServices.Marshal.PtrToStructure<T>(new IntPtr(pb));

                        }
                        return true;
                    }
                }
            }

        }
    }
}