using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace ConsulConfigurationBuilder
{
    public class ConsulConfigurationProvider : ConfigurationProvider
    {
        private readonly IList<string> _consulUris;
        private readonly IConsulHttpApi _consulHttpApi;
        private readonly ConsulJsonParser _consulJsonParser;
        private Dictionary<string, int> _urlAndIndex = new Dictionary<string, int>();
        private List<Task> lstTasks = new List<Task>();
        public ConsulConfigurationProvider(IList<string> consulUris, IConsulHttpApi consulHttpApi, ConsulJsonParser consulJsonParser)
        {
            _consulUris = consulUris;
            _consulHttpApi = consulHttpApi;
            _consulJsonParser = consulJsonParser;
            foreach (var consulUri in consulUris)
            {
                lstTasks.Add(new Task(o =>
                {
                    ListenOnChanges(consulUri);
                }, new object()));
            }
        }
        public override void Load()
        {
            foreach (var consulUri in _consulUris)
            {
                var index = InternalLoad(consulUri);
                _urlAndIndex.Add(consulUri, index);
            }

            foreach (var t in lstTasks)
            {
                if (t.Status == TaskStatus.Created)
                    t.Start();
            }
        }

        private void LoadChanges()
        {
            foreach (var consulUri in _consulUris)
            {
                var index = InternalLoad(consulUri);
                _urlAndIndex[consulUri] = index;
            }
        }
        private int InternalLoad(string consulUri)
        {

            var data = _consulHttpApi.GetKV(consulUri).GetAwaiter().GetResult();
            if (data != Stream.Null)
            {
                using (var reader = new StreamReader(data))
                {
                    var stringBuilder = new StringBuilder();
                    while (!reader.EndOfStream)
                    {
                        stringBuilder.Append(reader.ReadLine());
                    }
                    var jsonData = JArray.Parse(stringBuilder.ToString());
                    var index = int.Parse(jsonData[0]["ModifyIndex"].Value<string>());
                    Data = _consulJsonParser.Parse(
                        Encoding.Default.GetString(Convert.FromBase64String(jsonData[0]["Value"].Value<string>())));
                    return index;
                }
            }

            return 0;
        }

        private void ListenOnChanges(string consulUri)
        {

            while (true)
            {
                var index = _urlAndIndex[consulUri];
                var newConsul = consulUri + "?index=" + index;
                var data = _consulHttpApi.GetKV(newConsul).GetAwaiter().GetResult();
                if (data == Stream.Null)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
                else
                {
                    LoadChanges();
                }
            }
        }
    }
}