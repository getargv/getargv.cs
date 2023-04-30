namespace Getargv.Tests;

class TestHelper {
    public static string[] args() {
        return new string[]{
            Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess()?.MainModule?.FileName ?? "/usr/local/Cellar/dotnet/7.0.100/libexec/dotnet",
            "exec",
            "--runtimeconfig",
            $"{Environment.CurrentDirectory}/Getargv.Tests.runtimeconfig.json",
            "--depsfile",
            $"{Environment.CurrentDirectory}/Getargv.Tests.deps.json"
        }.Concat(Environment.GetCommandLineArgs()).ToArray();
    }
    public static string argString(string sep) {
        return string.Join(sep, args()) + "\0";
    }
    public static byte[] argBytes(string sep) {
        return Encoding.ASCII.GetBytes( argString(sep) );
    }
    public static byte[][] argBytesArray() {
        return args().Select(s => Encoding.ASCII.GetBytes(s+"\0")).ToArray();
    }
}
