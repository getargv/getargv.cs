using Getargv;
using System.Text;

namespace Getargv.Tests;

public class UnitTest1
{
    [Fact]
    public void goodPidShouldNotRaiseError()
    {
        var ex = Record.Exception(() => Getargv.asString(Environment.ProcessId, Encoding.UTF8));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(-1, typeof(ArgumentOutOfRangeException))]
    [InlineData(0, typeof(UnauthorizedAccessException))]
    [InlineData(1, typeof(UnauthorizedAccessException))]
    public void badPidsShouldRaiseErrors(int pid, Type exceptionType)
    {
        Exception ex = Assert.ThrowsAny<Exception>(() => Getargv.asString(pid, Encoding.UTF8));
        Assert.Equal(exceptionType, ex.GetType());
    }

}
