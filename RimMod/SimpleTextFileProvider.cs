using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace RimMod
{
    public class SimpleTextFileProvider : ConfigurationProvider
    {
        private readonly string _filePath;
        private readonly string _configPath;

        public SimpleTextFileProvider(string filePath, string configPath)
        {
            _filePath = filePath;
            _configPath = configPath;
        }

        public override void Load()
        {
            var dict = File.ReadAllLines(_filePath)
                .Select(x => TrimCommentsAndWhitespaces(x))
                .Where(x => !string.IsNullOrWhiteSpace(x))

                .Select((x, i) => new { x, i }).ToDictionary(x => _configPath + ":" + x.i, x => x.x);
            Data = dict;
        }

        private string TrimCommentsAndWhitespaces(string input)
        {
            var commentIdx = input.IndexOf("#");
            if (commentIdx >= 0)
                input = input.Substring(0, commentIdx);
            return input.Trim();
        }
    }
}
