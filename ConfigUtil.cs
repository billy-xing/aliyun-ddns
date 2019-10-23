using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Luna.Net.DDNS.Aliyun
{
    public class ConfigUtil
    {
        private static IConfigurationRoot _configuration = null;
        static ConfigUtil()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        public static string GetConfigVariableValue(string key, string defaultValue=null)
        {
            var envVar = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(envVar))
                return envVar;

            var appCfg = _configuration.GetSection("AppSettings");
            envVar = appCfg[key];
            if (!string.IsNullOrEmpty(envVar))
                return envVar;
            else
                return defaultValue;
        }
    }
}
