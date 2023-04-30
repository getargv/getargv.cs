using Getargv;

namespace Getargv.Tests;

#pragma warning disable CA1416
public class GetargvTests {
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

    [Theory]
    [InlineData("\0", false)]
    [InlineData(" ", true)]
    public void asBytesShouldReturnCorrectBytes(string sep, bool nuls)
    {
        var bytes = Getargv.asBytes(Environment.ProcessId, nuls);
        Assert.Equal(TestHelper.argBytes(sep), bytes);
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

    [Theory]
    [InlineData("\0", false)]
    [InlineData(" ", true)]
    public void asStringShouldReturnCorrectString(string sep, bool nuls)
    {
        var str = Getargv.asString(Environment.ProcessId, Encoding.ASCII, nuls);
        Assert.Equal(TestHelper.argString(sep), str);
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
        Assert.Equal(TestHelper.argBytesArray(), array);
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
        Assert.Equal(TestHelper.args(), array);
    }
}
#pragma warning restore CA1416
