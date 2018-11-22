using System.Collections.Generic;

namespace ConsulConfigurationBuilder
{
    public class ConsulJsonParser
    {
        public Dictionary<string, string> Parse(string data)
        {
            return JsonHelper.DeserializeAndFlatten(data);
        }

    }
}