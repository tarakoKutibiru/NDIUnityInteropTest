﻿using Microsoft.Win32.SafeHandles;
using NewTek;
using System;
using System.Runtime.InteropServices;

namespace NDI.Interop
{

    public enum Bandwidth
    {
        MetadataOnly = -10,
        AudioOnly = 10,
        Lowest = 0,
        Highest = 100
    }

    public enum ColorFormat
    {
        BGRX_BGRA = 0,
        UYVY_BGRA = 1,
        RGBX_RGBA = 2,
        UYVY_RGBA = 3,
        BGRX_BGRA_Flipped = 200,
        Fastest = 100
    }

    public class Recv : SafeHandleZeroOrMinusOneIsInvalid
    {
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct Settings
        {
            public Source Source;
            public ColorFormat ColorFormat;
            public Bandwidth Bandwidth;
            [MarshalAsAttribute(UnmanagedType.U1)]
            public bool AllowVideoFields;
            public IntPtr Name;
        }

        #region SafeHandle implementation

        Recv() : base(true) { }

        protected override bool ReleaseHandle()
        {
            _Destroy(handle);
            return true;
        }

        #endregion

        #region Public methods

        public static Recv Create(in Settings settings)
        {
            if (!NDIlib.initialize())
            {
                if (!NDIlib.is_supported_CPU())
                    UnityEngine.Debug.LogError("CPU incompatible with NDI.");
                else
                    UnityEngine.Debug.LogError("Unable to initialize NDI.");
                return null;
            }

            return _Create(settings);
        }

        ~Recv()
        {
            NDIlib.destroy();
        }

        public FrameType Capture
          (out VideoFrame video, IntPtr audio, IntPtr metadata, uint timeout)
          => _Capture(this, out video, audio, metadata, timeout);

        public void FreeVideoFrame(in VideoFrame frame)
          => _FreeVideo(this, frame);

        #endregion
        #region Unmanaged interface

        [DllImport(Config.DllName, EntryPoint = "NDIlib_recv_create_v3")]
        static extern Recv _Create(in Settings Settings);

        [DllImport(Config.DllName, EntryPoint = "NDIlib_recv_destroy")]
        static extern void _Destroy(IntPtr recv);

        [DllImport(Config.DllName, EntryPoint = "NDIlib_recv_capture_v2")]
        static extern FrameType _Capture(Recv recv,
          out VideoFrame video, IntPtr audio, IntPtr metadata, uint timeout);

        [DllImport(Config.DllName, EntryPoint = "NDIlib_recv_free_video_v2")]
        static extern void _FreeVideo(Recv recv, in VideoFrame data);

        #endregion
    }

}
