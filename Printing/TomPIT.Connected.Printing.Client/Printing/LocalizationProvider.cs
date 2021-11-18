using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Connected.Printing.Client.Configuration;

namespace TomPIT.Connected.Printing.Client.Printing
{
    public class LocalizationProvider: IDisposable
    {
        private readonly IMemoryCache _memoryCache;
        private HttpClient _client;
        private readonly SemaphoreSlim _semaphore;

        public LocalizationProvider(IMemoryCache memoryCache) 
        {
            _memoryCache = memoryCache;
            _client = new HttpClient();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.HttpHeaderBearer, Settings.Token);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.MimeTypeJson));
            _client.Timeout = TimeSpan.FromSeconds(5);

            _semaphore = new SemaphoreSlim(1, 1);
        }

        private static Uri _localizationUri => new Uri(new Uri(Settings.CdnUrl), "sys/localization/localize");
       
        internal async Task<string> GetLocalization(string microservice, string stringTable, string key, Guid identity)
        {          
            return await _memoryCache.GetOrCreateAsync<string>(LocalizedStringData.GetCacheKey(microservice, stringTable, identity, key), async (entry) =>
            {
                await _semaphore.WaitAsync();

                entry.SlidingExpiration = TimeSpan.FromDays(1);

                try
                {
                    var result = await _client.PostAsync(_localizationUri, new StringContent(JsonConvert.SerializeObject(new
                    {
                        MicroService = microservice,
                        StringTable = stringTable,
                        StringKey = key,
                        Identity = identity
                    }), Encoding.UTF8, Constants.MimeTypeJson));

                    if (!result.IsSuccessStatusCode)
                        return key;

                    var content = await result.Content.ReadAsStringAsync();

                    var localizationData = JsonConvert.DeserializeObject<LocalizedStringValue>(content);

                    
                    return localizationData.Value;
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex, LoggingLevel.Fatal);
                    return key;
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private class LocalizedStringData
        {
            public static string GetCacheKey(string microservice, string stringTable, Guid identity, string key) => $"{microservice}/{stringTable}/{identity}/{key}";
        }

        private class LocalizedStringValue 
        {
            public int LCID { get; set; }
            public string Value { get; set; }            
        }
    }
}
