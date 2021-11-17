using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Connected.Printing.Client.Configuration;

namespace TomPIT.Connected.Printing.Client.Printing
{
    public class LocalizationProvider
    {
        private static Uri _localizationUri => new Uri(new Uri(Settings.CdnUrl), "localization/localize");
        internal async Task<string> GetLocalization(string microservice, string stringTable, string key, string userToken = null)
        {
            try
            {
                var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.HttpHeaderBearer, Settings.Token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.MimeTypeJson));

                var result = await client.PostAsync(_localizationUri, new StringContent(JsonConvert.SerializeObject(new
                {
                    MicroService = microservice,
                    StringTable = stringTable,
                    StringKey = key,
                    UserToken = userToken
                }), Encoding.UTF8, Constants.MimeTypeJson));

                var content = await result.Content.ReadAsStringAsync();

                if (string.Compare(content, "null", true) == 0
                    || string.IsNullOrWhiteSpace(content))
                    return key;

                return content;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex, LoggingLevel.Fatal);
                return key;
            }
        }

        private class LocalizedStringData
        {
            public string GetCacheKey => $"{MicroService}/{StringTable}/{LCID}/{Key}";

            public string MicroService { get; set; }
            public string StringTable { get; set; }
            public string Key { get; set; }
            public int LCID { get; set; }
            public List<string> AssociatedUsers { get; } = new List<string>();
            public string LocalizedValue { get; set; }
        }
    }
}
