using System.Net.Http;

namespace RimMod.Helpers;

public static class HttpClientFactoryExtensions
{
    public static HttpClient Create<T>(this IHttpClientFactory factory)
    {
        return factory.CreateClient(typeof(T).Name);
    }
}