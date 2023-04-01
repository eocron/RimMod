using FluentAssertions;
using Moq;
using NUnit.Framework;
using SteamWorkshopSynchronizer.Commands;
using SteamWorkshopSynchronizer.Steam;

namespace SteamWorkshopSynchronizer.UnitTests
{
    [TestFixture(Category = "Integration")]
    public class SteamCmdClientTests
    {
        private string _downloadedFolder;
        private Uri _downloadLink;
        private IAsyncJob _job;
        private string _tmpFolder;
        private string _rootFolder;
        private CancellationTokenSource _cts;
        private SteamCmdProcessJob _steamCmdProcess;
        private ISteamCmdClient _steamCmdClient;
        private Task _task;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var mock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            mock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns<string>((x) =>
            {
                var client = new HttpClient();
                return client;
            });
            _downloadLink = new Uri("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip");
            _rootFolder = Path.Combine(Path.GetTempPath(), "tests", nameof(WindowsSteamCmdDownloadJobTests));
            _downloadedFolder = Path.Combine(_rootFolder, "downloaded");
            _tmpFolder = Path.Combine(_rootFolder, "temp");
            _steamCmdProcess =
                new SteamCmdProcessJob(Path.Combine(_downloadedFolder, "steamcmd.exe"), new TestLogger());
            _job = new CompoundAsyncJob(false,
                new WindowsSteamCmdDownloadJob(
                    mock.Object,
                    "foobar",
                    new TestLogger(),
                    _downloadLink,
                    _downloadedFolder,
                    _tmpFolder,
                    false),
                _steamCmdProcess);
            _steamCmdClient = new SteamCmdClient(_steamCmdProcess);
            _cts = new CancellationTokenSource();
            
            _task = Task.Run(() => _job.RunAsync(_cts.Token), _cts.Token);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _cts.Cancel();
            await _task.ConfigureAwait(false);
        }

        [Test]
        public async Task LoginAnonymous()
        {
                await _steamCmdClient.LoginAnonymousAsync(_cts.Token);
        }
    }
}