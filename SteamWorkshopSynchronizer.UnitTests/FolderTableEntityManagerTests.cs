using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SteamWorkshopSynchronizer.Folder;
using SteamWorkshopSynchronizer.Steam;

namespace SteamWorkshopSynchronizer.UnitTests
{
    public class FolderTableEntityManagerTests
    {
        private ITableEntityProvider<SteamWorkshopTableEntity> _provider;
        private ITableEntityManager<SteamWorkshopTableEntity> _manager;
        private CancellationToken _ct;
        private string _tempFolder;
        private Fixture _fixture;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), "tests", nameof(FolderTableEntityManagerTests));
            var manager = new FolderTableEntityManager<SteamWorkshopTableEntity>(_tempFolder, new TestLogger());
            _provider = manager;
            _manager = manager;
            _ct = new CancellationToken();
            _fixture = new Fixture();
        }
        
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            SafeDelete(_tempFolder);
        }

        [SetUp]
        public void SetUp()
        {
            SafeDelete(_tempFolder);
        }

        [Test]
        public async Task Crud()
        {
            var entities = _fixture.CreateMany<SteamWorkshopTableEntity>(3).ToList();
            foreach (var e in entities)
            {
                await _manager.CreateEntityAsync(e, _ct);
            }
            await AssertEntities(entities);
            
            entities[0].ETag = DateTime.UtcNow.ToString();
            entities[0].EscapedTitle = "new_title";
            entities[0].FileId = 10000;
            await _manager.UpdateEntityAsync(entities[0], _ct);
            await AssertEntities(entities);

            await _manager.DeleteEntityAsync(entities[0].Key, _ct);
            entities.RemoveAt(0);
            await AssertEntities(entities);
        }

        private async Task AssertEntities(IEnumerable<SteamWorkshopTableEntity> entities)
        {
            var result = await _provider.GetAllEntitiesAsync(_ct);
            result.Should().BeEquivalentTo(entities);
        }

        [Test]
        public async Task GetNoEntities()
        {
            var result = await _provider.GetAllEntitiesAsync(_ct);
            result.Should().BeEmpty();
        }

        private static void SafeDelete(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}