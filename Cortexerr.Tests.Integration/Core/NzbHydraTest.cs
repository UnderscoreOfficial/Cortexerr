using Cortexerr.Core.Indexers;

namespace Cortexerr.Tests.Integration.Core;

public class NzbHydraTest
{
    [Fact]
    public async Task NzbHydra_Search_Lifecycle()
    {
        var results = await NzbHydra.TvSearch(72059);
        Assert.Null(results.error);
        Assert.NotNull(results.data);
    }
}
