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

    namespace Common
    {
        class SafeHandle : System.Runtime.InteropServices.SafeHandle
        {
            public SafeHandle(global::Windows.Win32.Foundation.HANDLE h, bool ownsHandle) : base(h.Value, ownsHandle)
            {
            }

            public SafeHandle(IntPtr invalidHandleValue, bool ownsHandle) : base(invalidHandleValue, ownsHandle)
            {
                IsInvalid = true;
            }

            public override bool IsInvalid { get; }

            protected override bool ReleaseHandle()
            {
                return true;
            }
        }

        public class ModelBase : System.ComponentModel.INotifyPropertyChanged
        {
            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            protected bool SetValue<T>(T value, ref T current, [System.Runtime.CompilerServices.CallerMemberNameAttribute] string name = null)
            {
                if (object.Equals(value, current))
                {
                    return false;
                }
                current = value;
                OnPropertyChanged(name);
                return true;
            }

            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }

        [System.ComponentModel.DefaultProperty(nameof(Value))]
        public class SimpleModel<T> : ModelBase
        {
            public T Value
            {
                get => _Value;
                internal set => SetValue(value, ref _Value);
            }
            private T _Value;
        }

        public class BoolModel : SimpleModel<bool> { }
        public class UShortModel : SimpleModel<ushort> { }
    }
}