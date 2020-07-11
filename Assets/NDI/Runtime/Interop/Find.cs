﻿using Microsoft.Win32.SafeHandles;
using NewTek;
using System;
using System.Runtime.InteropServices;

namespace NDI.Interop
{

    public class Find : SafeHandleZeroOrMinusOneIsInvalid
    {
        #region SafeHandle implementation

        Find() : base(true) { }

        protected override bool ReleaseHandle()
        {
            _Destroy(handle);
            return true;
        }

        #endregion

        #region Public methods

        public static Find Create()
        {
            if (!NDIlib.initialize())
            {
                if (!NDIlib.is_supported_CPU())
                    UnityEngine.Debug.LogError("CPU incompatible with NDI.");
                else
                    UnityEngine.Debug.LogError("Unable to initialize NDI.");
                return null;
            }

            return _Create(new Settings { ShowLocalSources = true });
        }

        ~Find()
        {
            NDIlib.destroy();
        }

        unsafe public Span<Source> CurrentSources
        {
            get
            {
                uint count;
                var array = _GetCurrentSources(this, out count);
                return new Span<Source>((void*)array, (int)count);
            }
        }

        #endregion

        #region Unmanaged interface

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct Settings
        {
            [MarshalAsAttribute(UnmanagedType.U1)] public bool ShowLocalSources;
            public IntPtr Groups;
            public IntPtr ExtraIPs;
        }

        [DllImport(Config.DllName, EntryPoint = "NDIlib_find_create_v2")]
        static extern Find _Create(in Settings settings);

        [DllImport(Config.DllName, EntryPoint = "NDIlib_find_destroy")]
        static extern void _Destroy(IntPtr find);

        [DllImport(Config.DllName, EntryPoint = "NDIlib_find_get_current_sources")]
        static extern IntPtr _GetCurrentSources(Find find, out uint count);

        #endregion
    }

}
