using Getargv;
using System.Text;

namespace Getargv.Tests;

#pragma warning disable CA1416
public class GetargvTests {
    string[] args() {
        return new string[]{
            Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess()?.MainModule?.FileName ?? "/usr/local/Cellar/dotnet/7.0.100/libexec/dotnet",
            "exec",
            "--runtimeconfig",
            $"{Environment.CurrentDirectory}/Getargv.Tests.runtimeconfig.json",
            "--depsfile",
            $"{Environment.CurrentDirectory}/Getargv.Tests.deps.json"
        }.Concat(Environment.GetCommandLineArgs()).ToArray();
    }

    [Fact]
    public void asBytesGoodPidShouldNotRaiseError()
    {
        var ex = Record.Exception(() => Getargv.asBytes(Environment.ProcessId));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(Getargv.PID_MAX+1, typeof(ArgumentOutOfRangeException))]
    [InlineData(-1, typeof(ArgumentOutOfRangeException))]
    [InlineData(0, typeof(UnauthorizedAccessException))]
    [InlineData(1, typeof(UnauthorizedAccessException))]
    public void asBytesBadPidsShouldRaiseErrors(int pid, Type exceptionType)
    {
        Exception ex = Assert.ThrowsAny<Exception>(() => Getargv.asBytes(pid));
        Assert.Equal(exceptionType, ex.GetType());
    }

    [Fact]
    public void asBytesShouldReturnCorrectBytes()
    {
        var bytes = Getargv.asBytes(Environment.ProcessId);
        Assert.Equal(Encoding.ASCII.GetBytes( string.Join("\0", args()) + "\0" ), bytes);
    }

    [Fact]
    public void asStringGoodPidShouldNotRaiseError()
    {
        var ex = Record.Exception(() => Getargv.asString(Environment.ProcessId, Encoding.UTF8));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(Getargv.PID_MAX+1, typeof(ArgumentOutOfRangeException))]
    [InlineData(-1, typeof(ArgumentOutOfRangeException))]
    [InlineData(0, typeof(UnauthorizedAccessException))]
    [InlineData(1, typeof(UnauthorizedAccessException))]
    public void asStringBadPidsShouldRaiseErrors(int pid, Type exceptionType)
    {
        Exception ex = Assert.ThrowsAny<Exception>(() => Getargv.asString(pid, Encoding.UTF8));
        Assert.Equal(exceptionType, ex.GetType());
    }

    [Fact]
    public void asStringShouldReturnCorrectString()
    {
        var str = Getargv.asString(Environment.ProcessId, Encoding.ASCII);
        Assert.Equal(string.Join("\0", args()) + "\0", str);
    }

    [Fact]
    public void asBytesArrayGoodPidShouldNotRaiseError()
    {
        var ex = Record.Exception(() => Getargv.asBytesArray(Environment.ProcessId));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(Getargv.PID_MAX+1, typeof(ArgumentOutOfRangeException))]
    [InlineData(-1, typeof(ArgumentOutOfRangeException))]
    [InlineData(0, typeof(UnauthorizedAccessException))]
    [InlineData(1, typeof(UnauthorizedAccessException))]
    public void asBytesArrayBadPidsShouldRaiseErrors(int pid, Type exceptionType)
    {
        Exception ex = Assert.ThrowsAny<Exception>(() => Getargv.asBytesArray(pid));
        Assert.Equal(exceptionType, ex.GetType());
    }

    [Fact]
    public void asBytesArrayShouldReturnCorrectByteArrays()
    {
        var array = Getargv.asBytesArray(Environment.ProcessId);
        Assert.Equal(args().Select(s => Encoding.ASCII.GetBytes(s+"\0")), array);
    }

    [Fact]
    public void asArrayGoodPidShouldNotRaiseError()
    {
        var ex = Record.Exception(() => Getargv.asArray(Environment.ProcessId, Encoding.UTF8));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(Getargv.PID_MAX+1, typeof(ArgumentOutOfRangeException))]
    [InlineData(-1, typeof(ArgumentOutOfRangeException))]
    [InlineData(0, typeof(UnauthorizedAccessException))]
    [InlineData(1, typeof(UnauthorizedAccessException))]
    public void asArrayBadPidsShouldRaiseErrors(int pid, Type exceptionType)
    {
        Exception ex = Assert.ThrowsAny<Exception>(() => Getargv.asArray(pid, Encoding.UTF8));
        Assert.Equal(exceptionType, ex.GetType());
    }

    [Fact]
    public void asArrayShouldReturnCorrectStrings()
    {
        var array = Getargv.asArray(Environment.ProcessId, Encoding.ASCII);
        Assert.Equal(args(), array);
    }

}
#pragma warning restore CA1416
