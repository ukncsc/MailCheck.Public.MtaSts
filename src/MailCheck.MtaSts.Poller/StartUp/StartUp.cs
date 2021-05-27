using Amazon.SimpleNotificationService;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Poller.Config;
using MailCheck.MtaSts.Poller.Dns;
using MailCheck.MtaSts.Poller.Parsing;
using MailCheck.MtaSts.Poller.Parsing.MailCheck.MtaSts.Poller.Parsing;
using MailCheck.MtaSts.Poller.Rules;
using MailCheck.MtaSts.Poller.Rules.Record;
using MailCheck.MtaSts.Poller.Rules.Records;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.MtaSts.Poller.StartUp
{
    public class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings serializerSetting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };

                serializerSetting.Converters.Add(new StringEnumConverter());

                return serializerSetting;
            };

            services
                .AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<PollHandler>()
                .AddTransient<IMtaStsProcessor, MtaStsProcessor>()
                .AddTransient<IDnsClient, Dns.DnsClient>()
                .AddTransient<IMtaStsPollerConfig, MtaStsPollerConfig>()
                .AddSingleton<IDnsNameServerProvider, LinuxDnsNameServerProvider>()
                .AddTransient<IEnvironmentVariables, EnvironmentVariables>()
                .AddTransient<IEnvironment, EnvironmentWrapper>()
                .AddTransient<IMtaStsRecordsParser, MtaStsRecordsParser>()
                .AddTransient<IMtaStsRecordParser, MtaStsRecordParser>()
                .AddTransient<ITagParser, VersionParser>()
                .AddTransient<ITagParser, PolicyVersionIdParser>()
                .AddTransient<IExtensionTagParser, ExtensionTagParser>()
                .AddTransient<IMtaStsRecordsEvaluator, MtaStsRecordsEvaluator>()
                .AddTransient<IEvaluator<MtaStsRecords>, Evaluator<MtaStsRecords>>()
                .AddTransient<IEvaluator<MtaStsRecord>, Evaluator<MtaStsRecord>>()
                .AddTransient<IRule<MtaStsRecords>, NoMtaStsRecord>()
                .AddTransient<IRule<MtaStsRecords>, OnlyOneMtaStsRecord>()
                .AddTransient<IRule<MtaStsRecord>, VersionTagIsRequired>()
                .AddLookupClient();
        }
    }
}
