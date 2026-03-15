using Cortexerr.Core.Downloaders;

namespace Cortexerr.Tests.Integration.Core;

public class SabnzbdTest
{
    // sabnzbd has this seperation between the queue and history so a bigger file will likely stay in the queue and test it
    // where a smaller one likely will finish and move to the history and test that since the logic is handling both i'm testing both
    public const string SMALL_NZB = "https://sabnzbd.org/tests/test_download_100MB.nzb";
    public const string LARGE_NZB = "https://sabnzbd.org/tests/test_download_1000MB.nzb";
    [Fact]
    public async Task Sabnzbd_Download_Lifecycle_Small()
    {
        var sabnzbd = new Sabnzbd(SMALL_NZB);
        var add = await sabnzbd.Add();
        Assert.Null(add.error);
        Assert.NotNull(add.data);
        Assert.NotNull(sabnzbd.nzo_id);

        var nzb = await sabnzbd.Nzb(false);
        Assert.Null(nzb.error);
        Assert.NotNull(nzb.data);

        var delete = await sabnzbd.Delete();
        Assert.Null(delete.error);
    }
    [Fact]
    public async Task Sabnzbd_Download_Lifecycle_Large()
    {
        var sabnzbd = new Sabnzbd(LARGE_NZB);
        var add = await sabnzbd.Add();
        Assert.Null(add.error);
        Assert.NotNull(add.data);
        Assert.NotNull(sabnzbd.nzo_id);

        var nzb = await sabnzbd.Nzb(false);
        Assert.Null(nzb.error);
        Assert.NotNull(nzb.data);

        var delete = await sabnzbd.Delete();
        Assert.Null(delete.error);
    }
    [Fact]
    public async Task Sabnzbd_Invalid_Add()
    {
        var sabnzbd = new Sabnzbd("example");
        var add = await sabnzbd.Add();
        Assert.NotNull(add.error);
    }
}
