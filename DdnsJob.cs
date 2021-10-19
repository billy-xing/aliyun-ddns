using Aliyun.Acs.Alidns.Model.V20150109;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using Newtonsoft.Json;
using Quartz;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace Luna.Net.DDNS.Aliyun
{
    public class DdnsJob : IJob
    {
        const string CACHEKEY_CachedIP = "CachedIP";
        public Task Execute(IJobExecutionContext context)
        {
            var publicIP = GetPublicIPEx();
            var cachedIP = CacheHelper.GetCacheValue<string>(CACHEKEY_CachedIP);

            if(!string.IsNullOrWhiteSpace(cachedIP) && cachedIP.Equals(publicIP, StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", ConfigUtil.GetConfigVariableValue("accessKeyId"), ConfigUtil.GetConfigVariableValue("accessSecret"));
            DefaultAcsClient client = new DefaultAcsClient(profile);
            var request = new DescribeDomainRecordsRequest();
            request.DomainName = ConfigUtil.GetConfigVariableValue("DomainName");
            request.TypeKeyWord = ConfigUtil.GetConfigVariableValue("DomainRecordType");
            request.RRKeyWord = ConfigUtil.GetConfigVariableValue("DomainRecordRR");
            try
            {
                var response = client.GetAcsResponse(request);
                if (response.TotalCount > 0)
                {
                    var rec = response.DomainRecords.FirstOrDefault(t=>t.RR.Equals(request.RRKeyWord, StringComparison.CurrentCultureIgnoreCase));
                    if (rec != null && rec.Value != publicIP)
                    {
                        var reqChange = new UpdateDomainRecordRequest();
                        reqChange.RecordId = rec.RecordId;
                        reqChange.RR = rec.RR;
                        reqChange.Type = rec.Type;
                        reqChange.Value = publicIP;

                        var respChange = client.GetAcsResponse(reqChange);
                        
                        CacheHelper.SetCacheValue(CACHEKEY_CachedIP, publicIP);

                        Console.WriteLine($"[{DateTime.Now}]:{rec.RR}.{rec.DomainName} Changed to IP {publicIP} success");
                    }
                }
            }
            catch (ServerException e)
            {
                Console.WriteLine($"[{DateTime.Now}]【Exception】{e}");
            }
            catch (ClientException e)
            {
                Console.WriteLine($"[{DateTime.Now}]【Exception】{e}");
            }

            return Task.CompletedTask;
        }

        public string GetPublicIP()
        {
            var host = "https://api.myip.com/";
            var url = "";

            var client = new RestClient(host);
            var resp = client.Get(new RestRequest(url));
            var result = JsonConvert.DeserializeAnonymousType(resp.Content, new { ip = string.Empty });
            if (result != null )
            {
                return result.ip;
            }
            return string.Empty;
        }

        public string GetPublicIPEx()
        {
            var publicIP = string.Empty;

            var lst = ConfigUtil.GetConfigVariableValue("PublicIPUrlList", "https://api.myip.com/").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            CancellationTokenSource cts = new CancellationTokenSource();
            Parallel.ForEach(lst, (url,state) =>
            {
                url = url.Trim();
                Stopwatch sw = Stopwatch.StartNew();
                try
                {
                    using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                    {
                        var content = client.GetAsync(url,cts.Token).Result.Content.ReadAsStringAsync().Result;
                        if (state.IsStopped)
                            return;
                        var ip = ExtractIP(content);
                        Console.WriteLine($"url:{url},ip:{ip},time:{sw.ElapsedMilliseconds}");
                        if (!string.IsNullOrEmpty(ip))
                        {
                            publicIP = ip;
                            state.Stop();
                            cts.Cancel();
                            return;
                        }
                    }
                }
                catch(TaskCanceledException)
                {

                }
                catch(AggregateException ex)
                {
                    foreach(var innerEx in ex.InnerExceptions)
                    {
                        if(!(innerEx is TaskCanceledException))
                        {
                            Console.WriteLine($"url:{url},time:{sw.ElapsedMilliseconds},error:{innerEx.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"url:{url},time:{sw.ElapsedMilliseconds},error:{ex.Message}");
                }
                
                sw.Stop();
                
            });

            return publicIP;

        }

        public string ExtractIP(string text)
        {
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"(((25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))");
            var m = r.Match(text);

            if (m.Success)
                return m.Value;
            return null;
        }
        
    }
}
