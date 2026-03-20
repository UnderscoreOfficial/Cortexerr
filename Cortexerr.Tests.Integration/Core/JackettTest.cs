using Cortexerr.Core.Indexers;

namespace Cortexerr.Tests.Integration.Core;

public class JackettTest
{
    [Fact]
    public async Task Jackett_Search_Lifecycle()
    {
        var results = await Jackett.TvSearch("example", 0);
        Assert.Null(results.error);
        Assert.NotNull(results.data);
    }
}
