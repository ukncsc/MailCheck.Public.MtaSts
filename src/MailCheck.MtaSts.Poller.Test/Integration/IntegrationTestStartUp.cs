using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Logging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Common.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.MtaSts.Poller.Test.Integration
{
    public class IntegrationTestStartUp : StartUp.StartUp, IStartUp
    {
        private readonly IServiceCollection _fakeServices;

        public IntegrationTestStartUp(IServiceCollection fakeServices)
        {
            _fakeServices = fakeServices;
        }

        public new void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services
                .AddConcreateHandlers()
                .AddSerilogLogging();

            foreach (ServiceDescriptor fakeService in _fakeServices)
            {
                List<ServiceDescriptor> servicesToRemove = services.Where(x => x.ServiceType == fakeService.ServiceType).ToList();
                foreach (ServiceDescriptor serviceToRemove in servicesToRemove)
                {
                    services.Remove(serviceToRemove);
                }

                services.Add(fakeService);
            }
        }
    }
}