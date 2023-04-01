using AutoFixture;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SteamWorkshopSynchronizer.Core;
using SteamWorkshopSynchronizer.Settings;
using SteamWorkshopSynchronizer.Steam;

namespace SteamWorkshopSynchronizer.UnitTests
{
    public class SteamWorkshopTableEntitySynchronizationTests
    {
        private CancellationToken _ct;
        private Fixture _fixture;
        private SynchronizationMode _mode;
        private IAsyncJob _sync;
        private Mock<ITableEntityProvider<SteamWorkshopTableEntity>> _sourceProvider;
        private Mock<ITableEntityProvider<SteamWorkshopTableEntity>> _targetProvider;
        private Mock<ITableEntityManager<SteamWorkshopTableEntity>> _targetManager;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _sourceProvider = new Mock<ITableEntityProvider<SteamWorkshopTableEntity>>();
            _targetProvider = new Mock<ITableEntityProvider<SteamWorkshopTableEntity>>();
            _targetManager = new Mock<ITableEntityManager<SteamWorkshopTableEntity>>();
            _mode = SynchronizationMode.Create | SynchronizationMode.Delete | SynchronizationMode.Update;
            _sync = new TableEntitySynchronizationAsyncCommand<SteamWorkshopTableEntity>(
                _sourceProvider.Object,
                _targetProvider.Object, 
                _targetManager.Object,
                _mode, 
                true,
                new TestLogger());
            _ct = new CancellationToken();
            _fixture = new Fixture();
        }

        [Test]
        public async Task FullSync()
        {
            var sourceSet = _fixture.CreateMany<SteamWorkshopTableEntity>(4).ToList();
            var targetSet = _fixture.CreateMany<SteamWorkshopTableEntity>(2).Concat(sourceSet.Take(2).Select(DeepClone)).ToList();
            targetSet.Last().ETag += "changed";
            SetEntities(sourceSet, targetSet);
            
            await _sync.RunAsync(_ct);
            
            _targetManager.Verify(
                x => x.CreateEntityAsync(It.IsAny<SteamWorkshopTableEntity>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            _targetManager.Verify(
                x => x.UpdateEntityAsync(It.IsAny<SteamWorkshopTableEntity>(), It.IsAny<CancellationToken>()),
                Times.Exactly(1));
            _targetManager.Verify(
                x => x.DeleteEntityAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        private static T DeepClone<T>(T obj)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj))!;
        }

        private void SetEntities(List<SteamWorkshopTableEntity> sourceSet, List<SteamWorkshopTableEntity> targetSet)
        {
            _sourceProvider.Reset();
            _targetProvider.Reset();
            _sourceProvider.Setup(x => x.GetAllEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sourceSet);
            _targetProvider.Setup(x => x.GetAllEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(targetSet);
        }
    }
}