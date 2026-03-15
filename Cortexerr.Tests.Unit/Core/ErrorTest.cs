using Cortexerr.Core.Errors;

namespace Cortexerr.Tests.Unit.Core;

public class ErrorTest
{
    [Fact]
    public void Error_Sync_No_Return()
    {
        var result = Error.Handle(() => { });
        Assert.Null(result.error);
    }
    [Fact]
    public async Task Error_Async_No_Return()
    {
        var result = await Error.HandleAsync(async () => { });
        Assert.Null(result.error);
    }
    [Fact]
    public void Error_Sync_No_Return_Error()
    {
        var result = Error.Handle(() => throw new Exception());
        Assert.NotNull(result.error);
    }
    [Fact]
    public async Task Error_Async_No_Return_Error()
    {
        var result = await Error.HandleAsync(async () => throw new Exception());
        Assert.NotNull(result.error);
    }
    [Fact]
    public void Error_Sync_Return()
    {
        var result = Error.Handle<string>(() => "example");
        Assert.Null(result.error);
        Assert.NotNull(result.data);
    }
    [Fact]
    public async Task Error_Async_Return()
    {
        var result = await Error.HandleAsync<string>(async () =>
        {
            await Task.CompletedTask; return "example";
        });
        Assert.Null(result.error);
        Assert.NotNull(result.data);
    }
    [Fact]
    public void Error_Sync_Return_Error()
    {
        var result = Error.Handle<string>(() => throw new Exception());
        Assert.NotNull(result.error);
        Assert.Null(result.data);
    }
    [Fact]
    public async Task Error_Async_Return_Error()
    {
        var result = await Error.HandleAsync<string>(async () => throw new Exception());
        Assert.NotNull(result.error);
        Assert.Null(result.data);
    }
    [Fact]
    public void Response_Success_No_Data()
    {
        var result = Response.Success();
        Assert.Null(result.error);
        Assert.Null(result.data);
    }
    [Fact]
    public void Response_Success_With_Data()
    {
        var result = Response.Success("example");
        Assert.Null(result.error);
        Assert.NotNull(result.data);
    }
    [Fact]
    public void Response_Error_Valid_Input()
    {
        var result = Response.Error(ErrorCode.UNEXPECTED_ERROR, "example");
        Assert.NotNull(result.error);
        Assert.Null(result.data);
    }
}
