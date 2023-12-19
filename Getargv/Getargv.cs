using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Getargv;

[SupportedOSPlatform("macos")]
[StructLayout(LayoutKind.Sequential)]
struct GetArgvOptions {
    public uint skip;
    public int  pid;
    public bool nuls;
}

[SupportedOSPlatform("macos")]
[StructLayout(LayoutKind.Sequential)]
unsafe struct ArgvArgcResult {
    public byte*  buffer;
    public byte** argv;
    public uint   argc;
}

[SupportedOSPlatform("macos")]
[StructLayout(LayoutKind.Sequential)]
unsafe struct ArgvResult {
    public byte* buffer;
    public byte* start_pointer;
    public byte* end_pointer;
}

/// <summary>
/// Class that provides access to wrapped functions from libgetargv.
/// </summary>
[SupportedOSPlatform("macos")]
public static class Getargv
{
    /// <summary>
    /// The maximum value a pid may have on macOS unless you've compiled a custom xnu kernel.
    /// </summary>
    /// <value>99,999</value>
    /// <remarks>
    /// Per <see href="https://github.com/dotnet/core/blob/main/release-notes/7.0/supported-os.md#macos">the docs</see> dotnet only supports macOS &gt;= 10.15, so there's no need for &lt;= 10.5 support which would make this constant 30,000 on those versions.
    /// </remarks>
    public const int PID_MAX = 99999;
    const int EPERM = 1;
    const int ESRCH = 3;
    const int ENOMEM = 12;
    const int ERANGE = 34;
    const int ENAMETOOLONG = 63;

    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    static extern bool get_argv_of_pid(in GetArgvOptions options, out ArgvResult result);
    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    static extern bool get_argv_and_argc_of_pid(nint pid, out ArgvArgcResult result);
    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    static extern void free_ArgvArgcResult(ref ArgvArgcResult result);
    [DllImport("libgetargv.dylib", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    static extern void free_ArgvResult(ref ArgvResult result);

    /// <summary>
    /// Get the arguments of the process specified by <paramref name="pid"/> as a byte array,
    /// skipped ahead by <paramref name="skip"/> arguments, and with nuls converted to spaces
    /// if <paramref name="nuls"/> is true.
    /// </summary>
    /// <param name="pid">The process whose arguments shold be returned.</param>
    /// <param name="nuls">Convert nuls to spaces for human consumption.</param>
    /// <param name="skip">Number of leading arguments to skip past.</param>
    /// <returns>The arguments of the specified process <paramref name="pid"/>, formatted as requested</returns>
    /// <exception cref="OverflowException"> Arguments are longer than <see cref="Int32.MaxValue">Int32.MaxValue</see>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="pid"/> is &lt; 0 or &gt; <see cref="PID_MAX"/>, or if <paramref name="skip"/> is &gt; the number of args</exception>
    /// <exception cref="UnauthorizedAccessException">if you do not have permission to view the args of the targetted process <paramref name="pid"/></exception>
    /// <exception cref="ArgumentException">If <paramref name="pid"/> does not exist</exception>
    /// <exception cref="System.Data.DataException">If the arguments of <paramref name="pid"/> are malformed</exception>
    /// <exception cref="InsufficientMemoryException">If malloc fails to allocate memory</exception>
    /// <exception cref="NotImplementedException">If an unexpected errno is encountered</exception>
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
                if ((IntPtr)res.start_pointer == IntPtr.Zero || (IntPtr)res.end_pointer == IntPtr.Zero || res.start_pointer == res.end_pointer) return Array.Empty<byte>();
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
                case ENAMETOOLONG: throw new System.Data.DataException($"Arguments of PID {pid} are malformed");
                case ENOMEM: throw new InsufficientMemoryException("Failed to allocate memory");
                case ERANGE: throw new ArgumentOutOfRangeException(nameof(skip),"Skipping more args than process has");
                default: throw new NotImplementedException("Unknown errno encountered");
            }
        }
    }

    /// <summary>
    /// Get the arguments of the process specified by <paramref name="pid"/> as a string with <paramref name="encoding"/>,
    /// skipped ahead by <paramref name="skip"/> arguments, and with nuls converted to spaces if <paramref name="nuls"/> is true.
    /// </summary>
    /// <param name="pid">The process whose arguments shold be returned.</param>
    /// <param name="encoding">The encoding to attempt to use to read the arguments of the process.</param>
    /// <param name="nuls">Convert nuls to spaces for human consumption.</param>
    /// <param name="skip">Number of leading arguments to skip past.</param>
    /// <returns>The arguments of the specified process <paramref name="pid"/>, formatted as requested</returns>
    /// <exception cref="OverflowException"> Arguments are longer than <see cref="Int32.MaxValue">Int32.MaxValue</see>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="pid"/> is &lt; 0 or &gt; <see cref="PID_MAX"/>, or if <paramref name="skip"/> is &gt; the number of args</exception>
    /// <exception cref="UnauthorizedAccessException">if you do not have permission to view the args of the targetted process <paramref name="pid"/></exception>
    /// <exception cref="ArgumentException">If <paramref name="pid"/> does not exist, or if the arguments contain invalid Unicode code points.</exception>
    /// <exception cref="System.Data.DataException">If the arguments of <paramref name="pid"/> are malformed</exception>
    /// <exception cref="InsufficientMemoryException">If malloc fails to allocate memory</exception>
    /// <exception cref="NotImplementedException">If an unexpected errno is encountered</exception>
    /// <exception cref="ArgumentNullException">If the argument bytes are null</exception>
    /// <exception cref="ArgumentNullException">If the encoding argument is null</exception>
    /// <exception cref="System.Text.DecoderFallbackException">
    /// If a decoding fallback occurred (for more information, see <see href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/character-encoding">Character Encoding in .NET</see>) -and- <see cref="System.Text.Encoding.DecoderFallback">DecoderFallback</see> is set to <see cref="System.Text.DecoderExceptionFallback">DecoderExceptionFallback</see>.
    ///</exception>
    public static string asString(int pid, System.Text.Encoding encoding, bool nuls = false, uint skip = 0)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        return encoding.GetString(asBytes(pid, nuls, skip));
    }

    /// <summary> Get the arguments of the process specified by <paramref name="pid"/> as an array of byte arrays</summary>
    /// <param name="pid">The process whose arguments shold be returned.</param>
    /// <returns>The arguments of the specified process <paramref name="pid"/> as byte arrays</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="pid"/> is &lt; 0 or &gt; <see cref="PID_MAX"/></exception>
    /// <exception cref="UnauthorizedAccessException">if you do not have permission to view the args of the targetted process <paramref name="pid"/></exception>
    /// <exception cref="ArgumentException">If <paramref name="pid"/> does not exist</exception>
    /// <exception cref="System.Data.DataException">If the arguments of <paramref name="pid"/> are malformed</exception>
    /// <exception cref="InsufficientMemoryException">If malloc fails to allocate memory</exception>
    /// <exception cref="NotImplementedException">If an unexpected errno is encountered</exception>
    public static byte[][] asBytesArray(int pid)
    {
        if (pid < 0 || pid > PID_MAX) throw new ArgumentOutOfRangeException($"pid {pid} out of range");
        ArgvArgcResult res = new ArgvArgcResult();
        if (get_argv_and_argc_of_pid(pid, out res)) {
            int ptrSize = Marshal.SizeOf(typeof(IntPtr));
            int byteSize = Marshal.SizeOf(typeof(byte));
            byte[][] ret = new byte[res.argc][];
            for (uint i = 0; i < res.argc; i++) {
                unsafe {
                    byte* ptr = (byte*)Marshal.ReadIntPtr((IntPtr)res.argv, ptrSize * (int)i);
                    ulong len = 0;
                    if (i < res.argc-1) {
                        byte* nextptr = (byte*)Marshal.ReadIntPtr((IntPtr)res.argv, ptrSize * ((int)i+1));
                        len = (ulong)((nextptr-ptr)/byteSize);
                    } else {
                        while (Marshal.ReadByte((IntPtr)ptr, (int)len) != 0){len++;}
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
                case ENAMETOOLONG: throw new System.Data.DataException($"Arguments of PID {pid} are malformed");
                case ENOMEM: throw new InsufficientMemoryException("Failed to allocate memory.");
                default: throw new NotImplementedException("Unknown errno encountered");
            }
        }
    }

    /// <summary> Get the arguments of the process specified by <paramref name="pid"/> as an array of strings with <paramref name="encoding"/></summary>
    /// <param name="pid">The process whose arguments shold be returned.</param>
    /// <param name="encoding">The encoding to attempt to use to read the arguments of the process.</param>
    /// <returns>The arguments of the specified process <paramref name="pid"/> as a string array</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="pid"/> is &lt; 0 or &gt; <see cref="PID_MAX"/></exception>
    /// <exception cref="UnauthorizedAccessException">if you do not have permission to view the args of the targetted process <paramref name="pid"/></exception>
    /// <exception cref="ArgumentException">If <paramref name="pid"/> does not exist, or if the arguments contain invalid Unicode code points.</exception>
    /// <exception cref="System.Data.DataException">If the arguments of <paramref name="pid"/> are malformed</exception>
    /// <exception cref="InsufficientMemoryException">If malloc fails to allocate memory</exception>
    /// <exception cref="NotImplementedException">If an unexpected errno is encountered</exception>
    /// <exception cref="ArgumentNullException">If the argument bytes are null</exception>
    /// <exception cref="System.Text.DecoderFallbackException">
    /// If a decoding fallback occurred (for more information, see <see href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/character-encoding">Character Encoding in .NET</see>) -and- <see cref="System.Text.Encoding.DecoderFallback">DecoderFallback</see> is set to <see cref="System.Text.DecoderExceptionFallback">DecoderExceptionFallback</see>.
    /// </exception>
    public static string[] asArray(int pid, System.Text.Encoding encoding)
    {
        return Array.ConvertAll(asBytesArray(pid), b => encoding.GetString(b));
    }
}
