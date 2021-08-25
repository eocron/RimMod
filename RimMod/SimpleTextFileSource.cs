using Microsoft.Extensions.Configuration;

namespace RimMod
{
    public class SimpleTextFileSource : IConfigurationSource
    {
        private readonly string _filePath;
        private readonly string _configPath;

        public SimpleTextFileSource(string filePath, string configPath)
        {
            _filePath = filePath;
            _configPath = configPath;
        }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new SimpleTextFileProvider(_filePath, _configPath);
        }
    }
}
