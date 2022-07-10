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
    using global::Windows.Win32.UI.Input;
    using static global::Windows.Win32.PInvoke;

    namespace Input
    {
        using Input.HID;

        public sealed class InputTool : Common.ModelBase
        {
            /// <summary></summary>
            /// <param name="hwnd">WM_INPUTを受け取るウィンドウのハンドル</param>
            public InputTool(IntPtr hwnd)
            {
                RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[2];
                {
                    devices[0].usUsagePage = (ushort)HID.HID_UsagePage.GenericDesktopPage;
                    devices[0].usUsage = (ushort)HID.HID_GenericDesktop_ApplicationCollection.Gamepad;
                    devices[0].dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK | RAWINPUTDEVICE_FLAGS.RIDEV_EXINPUTSINK | RAWINPUTDEVICE_FLAGS.RIDEV_DEVNOTIFY;
                    devices[0].hwndTarget = new global::Windows.Win32.Foundation.HWND(hwnd);

                    devices[1].usUsagePage = (ushort)HID.HID_UsagePage.GenericDesktopPage;
                    devices[1].usUsage = (ushort)HID.HID_GenericDesktop_ApplicationCollection.Joystick;
                    devices[1].dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK | RAWINPUTDEVICE_FLAGS.RIDEV_EXINPUTSINK | RAWINPUTDEVICE_FLAGS.RIDEV_DEVNOTIFY;
                    devices[1].hwndTarget = new global::Windows.Win32.Foundation.HWND(hwnd);
                }

                uint size = (uint)System.Runtime.InteropServices.Marshal.SizeOf<RAWINPUTDEVICE>();

                // 通知を受け取れるように登録
                var ret = RegisterRawInputDevices(devices, size) == true;
            }

            /// <summary>見つかったデバイスの一覧</summary>
            public IEnumerable<HIDInputData> Devices
            {
                get => _Devices;
                private set => SetValue(value, ref _Devices);
            }
            private IEnumerable<HIDInputData> _Devices;

            private Dictionary<global::Windows.Win32.Foundation.HANDLE, HIDInputData> dic
                = new Dictionary<global::Windows.Win32.Foundation.HANDLE, HIDInputData>();


            public bool OnWindProc(int msg, IntPtr msg_wparam, IntPtr msg_lparam)
            {
                return OnWndProc(unchecked((uint)msg), msg_wparam, msg_lparam);
            }

            public bool OnWndProc(uint msg, IntPtr msg_wparam, IntPtr msg_lparam)
            {
                const int WM_INPUT_DEVICE_CHANGE = 0xFE;
                const int WM_INPUT = 0xFF;
                
                const int GIDC_ARRIVAL = 1;
                const int GIDC_REMOVAL = 2;

                if (msg == WM_INPUT_DEVICE_CHANGE)
                {
                    switch (msg_wparam.ToInt64())
                    {
                    case GIDC_ARRIVAL:
                        return false; //まだ何も実装してない

                    case GIDC_REMOVAL:
                        // デバイスが削除された
                        global::Windows.Win32.Foundation.HANDLE h = new global::Windows.Win32.Foundation.HANDLE(msg_lparam);
                        if (dic.TryGetValue(h, out var inputData))
                        {
                            // 登録済みなら既存インスタンス削除
                            dic.Remove(h);

                            // 一覧更新
                            Devices = dic.Values.ToArray();
                        }
                        return true;

                    default:
                        return false;
                    }
                }
                else if (msg != WM_INPUT)
                {
                    return false;
                }

                var data = OnWM_Input(msg_lparam);
                if (data == null || !data.IsHID)
                {
                    return false;
                }

                return true;
            }


            private InputData OnWM_Input(IntPtr msg_lparam)
            {
                // RawInputの取り出し
                if (!InputData.GetRawInputData(msg_lparam, out InputData inputData, out HIDInputData hidInputData))
                {
                    return null;
                }


                if (!inputData.IsHID && !(inputData is HIDInputData))
                {
                    // HIDではない
                    return inputData;
                }


                // 検出済みのインスタンスがあるか
                if (dic.TryGetValue(hidInputData.RawInput.header.hDevice, out HIDInputData cache))
                {
                    // HIDの生データから値の取り出し
                    cache.Parse(hidInputData);

                    return cache;//既存のインスタンスを返す
                }
                else
                {
                    // HIDの解析用設定を取得
                    if (!hidInputData.HasCaps && !hidInputData.GetCaps())
                    {
                        return null;
                    }

                    // HIDの生データから値の取り出し
                    hidInputData.Parse();

                    // デバイスのインスタンスを保持
                    dic.Add(hidInputData.RawInput.header.hDevice, hidInputData);

                    // 一覧更新
                    Devices = dic.Values.ToArray();

                    return hidInputData;
                }
            }

            #region static


            #endregion
        }
    }
}