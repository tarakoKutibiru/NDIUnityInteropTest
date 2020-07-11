﻿using Microsoft.Win32.SafeHandles;
using NewTek;
using System;
using System.Runtime.InteropServices;

namespace NDI.Interop
{

    public class Send : SafeHandleZeroOrMinusOneIsInvalid
    {
        #region SafeHandle implementation

        Send() : base(true) { }

        protected override bool ReleaseHandle()
        {
            _Destroy(handle);
            return true;
        }

        #endregion

        #region Public methods

        public static Send Create(string name)
        {
            if (!NDIlib.initialize())
            {
                if (!NDIlib.is_supported_CPU())
                    UnityEngine.Debug.LogError("CPU incompatible with NDI.");
                else
                    UnityEngine.Debug.LogError("Unable to initialize NDI.");
                return null;
            }

            var cname = Marshal.StringToHGlobalAnsi(name);
            var settings = new Settings { NdiName = cname };
            var ptr = _Create(settings);
            Marshal.FreeHGlobal(cname);
            return ptr;
        }

        ~Send()
        {
            NDIlib.destroy();
        }

        public void SendVideoAsync(in VideoFrame data)
          => _SendVideoAsync(this, data);

        #endregion

        #region Unmanaged interface

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct Settings
        {
            public IntPtr NdiName;
            public IntPtr Groups;
            [MarshalAsAttribute(UnmanagedType.U1)] public bool ClockVideo;
            [MarshalAsAttribute(UnmanagedType.U1)] public bool ClockAudio;
        }

        [DllImport(Config.DllName, EntryPoint = "NDIlib_send_create")]
        static extern Send _Create(in Settings settings);

        [DllImport(Config.DllName, EntryPoint = "NDIlib_send_destroy")]
        static extern void _Destroy(IntPtr send);

        [DllImport(Config.DllName, EntryPoint = "NDIlib_send_send_video_async_v2")]
        static extern void _SendVideoAsync(Send send, in VideoFrame data);

        #endregion
    }

}
