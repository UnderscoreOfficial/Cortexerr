using Cortexerr.Core.Logging;

namespace Cortexerr.Core.Errors;

public enum ErrorCode
{
    UNEXPECTED_ERROR, // Reserved for catched exceptions (should use exception error message)
    NOT_FOUND,
    ALREADY_EXISTS,
    INVALID_INPUT, // Used for when wrong values are used expected to be a recoverable state
    INVALID_STATE, // When state is wrong like methods called in a wrong sequence
    DISABLED, // only used for config based auth not a fail state but like missing envs for a service 
    REJECTED,
    TIMEOUT,
}

public record ErrorInfo
{
    public ErrorCode code { get; init; }
    public required string message { get; init; }
}

public record HandleResponse<T>
{
    public ErrorInfo? error { get; init; }
    public T? data { get; init; }
}

/// <summary>
/// Known sate response handeling used when success or error is known and
/// or an error state needs an explicit error code / message.
/// <list type="bullet">
/// <item>Response.Success - optionally takes data</item>
/// <item>Response.Error - requires ErrorCode and message</item>
/// </list>
/// <code>
///   @returns HandleResponse
/// </code>
/// </summary>
public static class Response
{
    public static HandleResponse<object> Success()
    {
        return new HandleResponse<object> { data = null };
    }
    public static HandleResponse<T> Success<T>()
    {
        return new HandleResponse<T> { data = default(T) };
    }
    public static HandleResponse<T> Success<T>(T data)
    {
        return new HandleResponse<T> { data = data };
    }
    public static HandleResponse<object> Error(ErrorCode code, string message)
    {
        Logger.Log.Error(message);
        return new HandleResponse<object>
        {
            error = new ErrorInfo
            {
                code = code,
                message = message
            }
        };
    }
    public static HandleResponse<T> Error<T>(ErrorCode code, string message)
    {
        Logger.Log.Error(message);
        return new HandleResponse<T>
        {
            error = new ErrorInfo
            {
                code = code,
                message = message
            }
        };
    }
}

/// <summary>
/// Takes a function sync / async and with / without return data that has no
/// params. Safely calls and handles the function avoiding any thrown errors.
/// Use when you expect an error to be thrown.
/// <list type="bullet">
/// <item>Error.Handle - synchronous error handler</item>
/// <item>Error.HandleAsync - asynchronous error handler</item>
/// </list>
/// <code>
///   e.g. var example = Error.Handle(() => { // code that may throw error });
///        if (example.error != null) {
///          // error state
///        }
///        if (example.data != null) {
///          // data state
///        }
///   When returning data the generic overload must match the return type.
///   @returns HandleResponse
/// </code>
/// </summary>
public static class Error
{
    // no data
    public static HandleResponse<object> Handle(Action method)
    {
        try
        {
            method();
            return Response.Success();
        }
        catch (Exception error)
        {
            return Response.Error(ErrorCode.UNEXPECTED_ERROR, error.ToString());
        }
    }

    // with data
    public static HandleResponse<TResult> Handle<TResult>(Func<TResult> method)
    {
        try
        {
            var result = method();
            return Response.Success(result);
        }
        catch (Exception error)
        {
            return Response.Error<TResult>(ErrorCode.UNEXPECTED_ERROR, error.ToString());
        }
    }

    // // async no data
    public static async Task<HandleResponse<object>> HandleAsync(Func<Task> method)
    {
        try
        {
            await method();
            return Response.Success();
        }
        catch (Exception error)
        {
            return Response.Error(ErrorCode.UNEXPECTED_ERROR, error.ToString());
        }
    }

    // // async with data
    public static async Task<HandleResponse<TResult>> HandleAsync<TResult>(
            Func<Task<TResult>> method
    )
    {
        try
        {
            var result = await method();
            return Response.Success(result);
        }
        catch (Exception error)
        {
            return Response.Error<TResult>(ErrorCode.UNEXPECTED_ERROR, error.ToString());
        }
    }
}
