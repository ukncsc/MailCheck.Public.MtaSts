using Amazon.SimpleNotificationService;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.PolicyFetcher;
using MailCheck.MtaSts.PolicyFetcher.Config;
using MailCheck.MtaSts.PolicyFetcher.Parsing;
using MailCheck.MtaSts.PolicyFetcher.Rules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.MtaSts.PolicyFetcher.StartUp
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
                .AddTransient<PolicyFetcherHandler>()
                .AddTransient<IPolicyFetcher, PolicyFetcher>()
                .AddTransient<IMtaStsPolicyFetcherConfig, MtaStsPolicyFetcherConfig>()
                .AddTransient<IEnvironmentVariables, EnvironmentVariables>()
                .AddTransient<IEnvironment, EnvironmentWrapper>()
                .AddTransient<IMtaStsPolicyParser, MtaStsPolicyParser>()
                .AddTransient<IKeyParser, ModeParser>()
                .AddTransient<IKeyParser, MxParser>()
                .AddTransient<IKeyParser, MaxAgeParser>()
                .AddTransient<IKeyParser, VersionParser>()
                .AddTransient<IEvaluator<MtaStsPolicyResult>, Evaluator<MtaStsPolicyResult>>()
                .AddTransient<IRule<MtaStsPolicyResult>, MtaStsShouldBeEnforced>()
                .AddTransient<IRule<MtaStsPolicyResult>, MaxAgeStrongKeyRule>();
        }
    }
}
