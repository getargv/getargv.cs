namespace Getargv.Tests;

using System.Runtime.InteropServices;
using System.Diagnostics;

class TestHelper {
    public static string[] args() {
        string homebrew_prefix = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "/opt/homebrew" : "/usr/local";
        string exe = Environment.ProcessPath ?? Process.GetCurrentProcess()?.MainModule?.FileName ?? $"{homebrew_prefix}/Cellar/dotnet/{Environment.Version}/libexec/dotnet";
        return new string[]{
            exe,
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
