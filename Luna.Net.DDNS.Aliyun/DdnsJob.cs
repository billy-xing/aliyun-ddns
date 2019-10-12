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

namespace Luna.Net.DDNS.Aliyun
{
    public class DdnsJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var publicIP = GetPublicIP();

            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", ConfigUtil.GetConfigVariableValue("accessKeyId"), ConfigUtil.GetConfigVariableValue("accessSecret"));
            DefaultAcsClient client = new DefaultAcsClient(profile);
            var request = new DescribeDomainRecordsRequest();
            //request.Value = "3.0.3.0";
            //request.Type = "A";
            //request.RR = "apitest1";
            request.DomainName = ConfigUtil.GetConfigVariableValue("DomainName");
            request.TypeKeyWord = ConfigUtil.GetConfigVariableValue("DomainRecordType");
            request.RRKeyWord = ConfigUtil.GetConfigVariableValue("DomainRecordRR");
            try
            {
                var response = client.GetAcsResponse(request);
                if (response.TotalCount > 0)
                {
                    var rec = response.DomainRecords[0];
                    if (rec.Value != publicIP)
                    {
                        var reqChange = new UpdateDomainRecordRequest();
                        reqChange.RecordId = rec.RecordId;
                        reqChange.Value = publicIP;

                        var respChange = client.GetAcsResponse(reqChange);
                        Console.WriteLine("Changed success");
                    }
                }
                // Console.WriteLine(System.Text.Encoding.Default.GetString(response.HttpResponse.Content));
            }
            catch (ServerException e)
            {
                Console.WriteLine(e);
            }
            catch (ClientException e)
            {
                Console.WriteLine(e);
            }

            return Task.CompletedTask;
        }

        public string GetPublicIP()
        {
            var host = "http://ip.taobao.com/";
            var url = "/service/getIpInfo2.php?ip=myip";

            var client = new RestClient(host);
            var resp = client.Get(new RestRequest(url));
            var result = JsonConvert.DeserializeAnonymousType(resp.Content, new { code = 0, data = new { ip = string.Empty } });
            if (result != null && result.code == 0)
            {
                return result.data.ip;
            }
            return string.Empty;
        }
    }
}
