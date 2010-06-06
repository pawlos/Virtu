﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace Jellyfish.Library
{
    [SecurityCritical]
    public abstract class SafeAllocHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected SafeAllocHandle(bool ownsHandle) : 
            base(ownsHandle)
        {
        }
    }

    [SecurityCritical]
    public sealed class SafeGlobalAllocHandle : SafeAllocHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public SafeGlobalAllocHandle() : 
            base(ownsHandle: true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public SafeGlobalAllocHandle(IntPtr existingHandle, bool ownsHandle) : 
            base(ownsHandle)
        {
            SetHandle(existingHandle);
        }

        [SecurityCritical]
        public static SafeGlobalAllocHandle Allocate(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var alloc = Allocate(value.Length);
            Marshal.Copy(value, 0, alloc.DangerousGetHandle(), value.Length);

            return alloc;
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags")]
        [SecurityCritical]
        public static SafeGlobalAllocHandle Allocate(int size, uint flags = 0x0)
        {
            var alloc = NativeMethods.GlobalAlloc(flags, (IntPtr)size);
            if (alloc.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            return alloc;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return (NativeMethods.GlobalFree(handle) == IntPtr.Zero);
        }

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern SafeGlobalAllocHandle GlobalAlloc(uint dwFlags, IntPtr sizetBytes);

            [DllImport("kernel32.dll", SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            public static extern IntPtr GlobalFree(IntPtr hMem);
        }
    }

    [SecurityCritical]
    public sealed class SafeLocalAllocHandle : SafeAllocHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public SafeLocalAllocHandle() : 
            base(ownsHandle: true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public SafeLocalAllocHandle(IntPtr existingHandle, bool ownsHandle) : 
            base(ownsHandle)
        {
            SetHandle(existingHandle);
        }

        [SecurityCritical]
        public static SafeLocalAllocHandle Allocate(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var alloc = Allocate(value.Length);
            Marshal.Copy(value, 0, alloc.DangerousGetHandle(), value.Length);

            return alloc;
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags")]
        [SecurityCritical]
        public static SafeLocalAllocHandle Allocate(int size, uint flags = 0x0)
        {
            var alloc = NativeMethods.LocalAlloc(flags, (IntPtr)size);
            if (alloc.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            return alloc;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return (NativeMethods.LocalFree(handle) == IntPtr.Zero);
        }

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern SafeLocalAllocHandle LocalAlloc(uint dwFlags, IntPtr sizetBytes);

            [DllImport("kernel32.dll", SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            public static extern IntPtr LocalFree(IntPtr hMem);
        }
    }
}
