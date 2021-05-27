﻿using System.Collections.Generic;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Data;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.FeatureManagement;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.SSM;
using MailCheck.Common.Util;
using MailCheck.MtaSts.Entity.Config;
using MailCheck.MtaSts.Entity.Dao;
using MailCheck.MtaSts.Entity.Entity;
using MailCheck.MtaSts.Entity.Entity.EmailSecurity;
using MailCheck.MtaSts.Entity.Entity.Notifiers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MailCheck.MtaSts.Entity.StartUp
{
    public class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () => SerialisationConfig.Settings;

            services
                .AddSingleton<IClock, Clock>()
                .AddSingleton<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddSingleton<IDatabase, DefaultDatabase<MySqlProvider>>()
                .AddSingleton<IEnvironment, EnvironmentWrapper>()
                .AddSingleton<IEnvironmentVariables, EnvironmentVariables>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddSingleton<IMtaStsEntityConfig, MtaStsEntityConfig>()
                .AddSingleton<IMtaStsEntityDao, MtaStsEntityDao>()
                .AddTransient<IEntityChangedPublisher, EntityChangedPublisher>()
                .AddTransient<IChangeNotifier, AdvisoryChangedNotifier>()
                .AddTransient<IEqualityComparer<AdvisoryMessage>, MessageEqualityComparer>()
                .AddConditionally(
                    "NewScheduler",
                    featureActiveRegistrations =>
                    {
                        featureActiveRegistrations.AddTransient<MtaStsEntityNewScheduler>();
                    },
                    featureInactiveRegistrations =>
                    {
                        featureInactiveRegistrations.AddTransient<MtaStsEntity>();
                    });
        }
    }
}