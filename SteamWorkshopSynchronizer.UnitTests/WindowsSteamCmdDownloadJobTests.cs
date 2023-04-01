using FluentAssertions;
using Moq;
using NUnit.Framework;
using SteamWorkshopSynchronizer.Steam;

namespace SteamWorkshopSynchronizer.UnitTests
{
    [TestFixture(Category = "Integration")]
    public class WindowsSteamCmdDownloadJobTests
    {
        private string _downloadedFolder;
        private Uri _downloadLink;
        private WindowsSteamCmdDownloadJob _job;
        private CancellationToken _ct;
        private string _tmpFolder;
        private string _rootFolder;

        [OneTimeSetUp]
        public void OneTimeSetUp()
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
            _job = new WindowsSteamCmdDownloadJob(
                mock.Object, 
                "foobar", 
                new TestLogger(), 
                _downloadLink,
                _downloadedFolder, 
                _tmpFolder,
                false);
            _ct = new CancellationToken();
            
            if (Directory.Exists(_rootFolder))
            {
                Directory.Delete(_rootFolder, true);
            }
        }

        [Test]
        public async Task Download()
        {
            await _job.RunAsync(_ct);
            File.Exists(Path.Combine(_downloadedFolder, "steamcmd.exe")).Should().BeTrue();
            Console.WriteLine(_downloadedFolder);
        }
    }
}