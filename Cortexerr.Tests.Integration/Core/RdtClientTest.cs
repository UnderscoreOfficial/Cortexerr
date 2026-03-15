using Cortexerr.Core.Downloaders;

namespace Cortexerr.Tests.Integration.Core;

public class RdtClientTest
{
    public const string MAGNET = "magnet:?xt=urn:btih:8e3f4283d1c8360d2f18544a8b166813086675a1&dn=archlinux-2026.02.01-x86_64.iso";
    [Fact]
    public async Task RdtClient_Download_Lifecycle()
    {
        var rdtclient = new RdtClient(MAGNET);
        var add = await rdtclient.Add();
        Assert.Null(add.error);
        Assert.NotNull(rdtclient.hash);

        var torrent = await rdtclient.Torrent(false);
        Assert.Null(torrent.error);
        Assert.NotNull(torrent.data);

        var delete = await rdtclient.Delete();
        Assert.Null(delete.error);
    }

    [Fact]
    public async Task RdtClient_Invalid_Add()
    {
        var rdtclient = new RdtClient("example");
        var add = await rdtclient.Add();
        Assert.NotNull(add.error);
    }
}
