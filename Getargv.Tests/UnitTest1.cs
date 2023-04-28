using Getargv;
using System.Text;

namespace Getargv.Tests;

public class GetargvTests {
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

}
