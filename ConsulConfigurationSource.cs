using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace ConsulConfigurationBuilder
{
    public class ConsulConfigurationSource : IConfigurationSource
    {
        private readonly IList<string> _consulUris;
        public ConsulConfigurationSource(IList<string> consulUris)
        {
            _consulUris = consulUris;
        }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConsulConfigurationProvider(_consulUris, new ConsulHttpApi(), new ConsulJsonParser());
        }
    }

    public static class ConsulConfigurationBuilderExtension
    {
        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder builder, IList<string> consulPath)
        {
            builder.Add(new ConsulConfigurationSource(consulPath));
            return builder;
        }
    }
}
