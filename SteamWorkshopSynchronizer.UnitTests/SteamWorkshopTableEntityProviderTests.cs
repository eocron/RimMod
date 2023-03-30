using FluentAssertions;
using Moq;
using NUnit.Framework;
using SteamWorkshopSynchronizer.Steam;

namespace SteamWorkshopSynchronizer.UnitTests;

public class SteamWorkshopTableEntityProviderTests
{
    private SteamWorkshopTableEntityProvider _provider;
    private CancellationToken _ct;
    private long[] _allFileIds;

    [OneTimeSetUp]
    public void Setup()
    {
        var mock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        mock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns<string>((x) =>
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.steampowered.com/");
            return client;
        });
        _allFileIds = new long[]
        {
            818773962,//hugslib
            2009463077//harmony
        };
        _provider = new SteamWorkshopTableEntityProvider(mock.Object, "foobar", _allFileIds);
        _ct = new CancellationToken();
    }

    [Test]
    public async Task GetAllEntities()
    {
        var result = await _provider.GetAllEntitiesAsync(_ct);
        result.Select(x => x.FileId).Should().BeEquivalentTo(_allFileIds);
    }
    
    [Test]
    public async Task GetSomeEntities()
    {
        var fileId = _allFileIds.First();
        var result = await _provider.GetEntityAsync(fileId.ToString(), _ct);
        result.FileId.Should().Be(fileId);
        result.Key.Should().Be(fileId.ToString());
        result.AppId.Should().BeGreaterThan(0);
        
    }
}