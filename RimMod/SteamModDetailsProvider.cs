using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RimMod
{
    public class SteamModDetailsProvider : ISteamModDetailsProvider
    {
        private readonly string SteamGetPublishedFileDetailsUrl = "https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/";

        private readonly IHttpClientFactory _httpClientFactory;

        public SteamModDetailsProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ModDetails> GetRemoteDetails(long fileId, CancellationToken cancellationToken)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string> {
                {"itemcount","1" },
                {"publishedfileids[0]", fileId.ToString() }
            });
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.PostAsync(SteamGetPublishedFileDetailsUrl, form, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var obj = JObject.Parse(json).SelectToken("$.response.publishedfiledetails[0]").ToObject<ModDetails>();
            return obj;
        }

        public async Task<ModDetails> GetLocalDetails(string folder, CancellationToken cancellationToken)
        {
            var path = Path.Combine(folder, "update_info.json");
            if (!File.Exists(path))
                return new ModDetails();
            var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<ModDetails>(json);
        }

        public async Task SaveLocalDetails(string folder, ModDetails details, CancellationToken cancellationToken)
        {
            var path = Path.Combine(folder, "update_info.json");
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(details), cancellationToken).ConfigureAwait(false);
        }
    }
}
