using System;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Logging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.Common.SSM;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Evaluator.Config;
using MailCheck.MtaSts.Evaluator.Explainers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MailCheck.MtaSts.Evaluator.StartUp
{
    public class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () => SerialisationConfig.Settings;

            services
                .AddTransient<IHandle<MtaStsRecordsPolled>, EvaluationHandler>()
                .AddTransient<IMtaStsEvaluationProcessor, MtaStsEvaluationProcessor>()
                .AddTransient<ITagExplainer, VersionTagExplainer>()
                .AddTransient<ITagExplainer, PolicyVersionIdTagExplainer>()
                .AddTransient<IMtaStsRecordExplainer, MtaStsRecordExplainer>()
                
                .AddTransient<IEvaluator<MtaStsRecord>, Evaluator<MtaStsRecord>>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<IMtaStsEvaluatorConfig, MtaStsEvaluatorConfig>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddSerilogLogging();
        }
    }
}