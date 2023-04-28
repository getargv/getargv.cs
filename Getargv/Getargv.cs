using System;
using System.Runtime.InteropServices;

namespace Getargv;

[SupportedOSPlatform("macos")]
[StructLayout(LayoutKind.Sequential)]
struct GetArgvOptions {
    public uint skip;
    public int  pid;
    public bool  nuls;
}

[SupportedOSPlatform("macos")]
[StructLayout(LayoutKind.Sequential)]
unsafe struct ArgvArgcResult {
    public byte*  buffer;
    public byte** argv;
    public nuint  argc;
}

[SupportedOSPlatform("macos")]
[StructLayout(LayoutKind.Sequential)]
unsafe struct ArgvResult {
    public byte* buffer;
    public byte* start_pointer;
    public byte* end_pointer;
}

[SupportedOSPlatform("macos")]
public static class Getargv
{
    public const int PID_MAX = 99999; // per https://github.com/dotnet/core/blob/main/release-notes/7.0/supported-os.md#macos only 10.15+ is supported, no need for 10.5 support
    const int EPERM = 1;
    const int ESRCH = 3;
    const int ENOMEM = 12;
    const int ERANGE = 34;
    const int ENAMETOOLONG = 63;

    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)] static extern bool print_argv_of_pid(in byte start_pointer, in byte end_pointer);
    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)] static extern bool get_argv_of_pid(in GetArgvOptions options, out ArgvResult result);
    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)] static extern bool get_argv_and_argc_of_pid(nint pid, out ArgvArgcResult result);
    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)] static extern void free_ArgvArgcResult(ref ArgvArgcResult result);
    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)] static extern void free_ArgvResult(ref ArgvResult result);

    public static byte[] asBytes(int pid, bool nuls = false, uint skip = 0)
    {
        if (pid < 0 || pid > PID_MAX) throw new ArgumentOutOfRangeException($"pid {pid} out of range");
        GetArgvOptions opt;
        opt.pid = pid;
        opt.skip = skip;
        opt.nuls = nuls;
        ArgvResult res = new ArgvResult();
        if (get_argv_of_pid(in opt, out res)) {
            unsafe {
                var len = Convert.ToInt32(res.end_pointer - res.start_pointer + 1);
                byte[] ret = new byte[len];
                Marshal.Copy((IntPtr)res.start_pointer, ret, 0, len);
                free_ArgvResult(ref res);
                return ret;
            }
        } else {
            //handle failure
            switch (Marshal.GetLastPInvokeError()) {
                case EPERM: throw new UnauthorizedAccessException($"Do not have permission to access args of PID: {pid}");
                case ESRCH: throw new ArgumentException($"PID {pid} does not exist", nameof(pid));
                case ENAMETOOLONG: throw new System.Data.DataException("Arguments of PID {pid} are malformed");
                case ENOMEM: throw new InsufficientMemoryException("Failed to allocate memory");
                case ERANGE: throw new ArgumentOutOfRangeException("Skipping more args than process has");
                default: throw new NotImplementedException("Unknown errno encountered.");
            }
        }
    }

    public static string asString(int pid, System.Text.Encoding encoding, bool nuls = false, uint skip = 0)
    {
        return encoding.GetString(asBytes(pid, nuls, skip));
    }

    public static byte[][] asBytesArray(int pid)
    {
        if (pid < 0 || pid > PID_MAX) throw new ArgumentOutOfRangeException($"pid {pid} out of range");
        ArgvArgcResult res = new ArgvArgcResult();
        if (get_argv_and_argc_of_pid(pid, out res)) {
            int elementSize = Marshal.SizeOf(typeof(IntPtr));
            byte[][] ret = new byte[res.argc][];
            for (nuint i = 0; i < res.argc; i++) {
                unsafe {
                    byte* ptr = (byte*)Marshal.ReadIntPtr((IntPtr)res.argv, elementSize * (int)i);
                    ulong len = 0;
                    if (i < res.argc-1) {
                        byte* nextptr = (byte*)Marshal.ReadIntPtr((IntPtr)res.argv, elementSize * ((int)i+1));
                        len = (ulong)((nextptr-ptr)/elementSize);
                    } else {
                        while (Marshal.ReadByte((IntPtr)ptr, (int)len)!=0){len++;}
                        len++;// len was index of nul, add one to get length of byte[] including nul
                    }
                    ret[i] = new byte[len];
                    Marshal.Copy((IntPtr)ptr, ret[i], 0, (int)len);
                }
            }
            free_ArgvArgcResult(ref res);
            return ret;
        } else {
            //handle failure
            switch (Marshal.GetLastPInvokeError()) {
                case EPERM: throw new UnauthorizedAccessException($"Do not have permission to access args of PID: {pid}");
                case ESRCH: throw new ArgumentException($"PID {pid} does not exist", nameof(pid));
                case ENAMETOOLONG: throw new System.Data.DataException("Arguments of PID {pid} are malformed");
                case ENOMEM: throw new InsufficientMemoryException("Failed to allocate memory.");
                default: throw new NotImplementedException("Unknown errno encountered.");
            }
        }
    }

    public static string[] asArray(int pid, System.Text.Encoding encoding)
    {
        return Array.ConvertAll(asBytesArray(pid), b => encoding.GetString(b));
    }

    // case EAGAIN: throw new __Exception($"printing to stdout failed");
}
