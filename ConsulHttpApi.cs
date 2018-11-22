using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsulConfigurationBuilder
{
    public interface  IConsulHttpApi
    {
        Task<Stream> GetKV(string uri);

    }

    public class ConsulHttpApi : IConsulHttpApi
    {

        public async Task<Stream> GetKV(string uri)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var httpResponseMessage = await httpClient.GetAsync(uri);
                    return await httpResponseMessage.Content.ReadAsStreamAsync();
                }
            }
            catch
            {
                return Stream.Null;
            }
        }
    }
}
