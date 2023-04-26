using System;
using System.Runtime.InteropServices;

namespace Getargv;

// PlatformNotSupportedException

[StructLayout(LayoutKind.Sequential)]
struct GetArgvOptions {
    public nuint skip;
    public nint  pid;
    public bool  nuls;
}

[StructLayout(LayoutKind.Sequential)]
unsafe struct ArgvArgcResult {
    public byte*  buffer;
    public byte** argv;
    public nuint  argc;
}

[StructLayout(LayoutKind.Sequential)]
unsafe struct ArgvResult {
    public byte* buffer;
    public byte* start_pointer;
    public byte* end_pointer;
}

public static class Getargv
{
    public const int PID_MAX = 99999; // per https://github.com/dotnet/core/blob/main/release-notes/7.0/supported-os.md#macos only 10.15+ is supported, no need for 10.5 support
    const int EPERM = 1;
    const int ESRCH = 3;
    const int ENOMEM = 12;
    const int ENAMETOOLONG = 63;

    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)] static extern bool print_argv_of_pid(ref byte start_pointer, ref byte end_pointer);
    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)] static extern bool get_argv_of_pid(ref GetArgvOptions options, ref ArgvResult result);
    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)] static extern bool get_argv_and_argc_of_pid(nint pid, ref ArgvArgcResult result);
    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)] static extern void free_ArgvArgcResult(ref ArgvArgcResult result);
    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)] static extern void free_ArgvResult(ref ArgvResult result);

    public static string asString(int pid, System.Text.Encoding encoding, bool nuls = false, uint skip = 0)
    {
        if (pid < 0 || pid > PID_MAX) throw new ArgumentOutOfRangeException($"pid {pid} out of range");
        GetArgvOptions opt = new GetArgvOptions();
        opt.pid = pid;
        opt.skip = skip;
        opt.nuls = nuls;
        ArgvResult res = new ArgvResult();
        if (get_argv_of_pid(ref opt, ref res)) {
            unsafe {
                string ret = encoding.GetString(res.start_pointer, Convert.ToInt32(res.end_pointer - res.start_pointer + 1));
                free_ArgvResult(ref res);
                return ret;
            }
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

    // case EAGAIN: throw new __Exception($"printing to stdout failed");
    // case ERANGE: throw new ArgumentOutOfRangeException($"skipping more args than exist range");
}
