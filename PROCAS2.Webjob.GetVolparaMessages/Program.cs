using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Unity;
using Unity.Lifetime;


using PROCAS2.Data;
using PROCAS2.Services.App;
using PROCAS2.Services.Utility;

namespace PROCAS2.Webjob.GetVolparaMessages
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            UnityContainer container = new UnityContainer();
            container.RegisterInstance<PROCAS2Context>(new PROCAS2Context());
            container.RegisterType(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            container.RegisterType<IUnitOfWork, UnitOfWork>();
            container.RegisterType<IContextService, ContextService>();
            container.RegisterType<IConfigService, ConfigService>();
            container.RegisterType<IHashingService, HashingService>();
            container.RegisterType<IParticipantService, ParticipantService>();
            container.RegisterType<IResponseService, ResponseService>();
            container.RegisterType<ICRAService, CRAService>();
            container.RegisterType<IPROCAS2UserManager, PROCASUserManager>();
            container.RegisterType<PROCAS2Context>(new PerResolveLifetimeManager());
            container.RegisterType<IWebJobParticipantService, WebJobParticipantService>();
            container.RegisterType<IWebJobLogger, WebJobLogger>();
            container.RegisterType<IAuditService, AuditService>();


            container.RegisterType<Functions>(); //Need to register WebJob class

            var config = new JobHostConfiguration()
            {
                JobActivator = new UnityJobActivator(container)
            };

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            config.UseServiceBus();

            var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }

    public class UnityJobActivator : IJobActivator
    {
        private readonly IUnityContainer _container;

        public UnityJobActivator(IUnityContainer container)
        {
            _container = container;
        }

        public T CreateInstance<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
