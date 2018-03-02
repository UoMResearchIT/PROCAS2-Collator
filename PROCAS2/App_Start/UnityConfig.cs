using System.Web.Mvc;
using System.Web.Http;
using System.Web;

using Microsoft.Practices.Unity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Data.Entity;
using PROCAS2.Data.Identity;
using PROCAS2.Data;
using PROCAS2.Controllers;
using PROCAS2.Services.Utility;
using PROCAS2.Services.App;

namespace PROCAS2
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            container.RegisterInstance<PROCAS2Context>(new PROCAS2Context());
            container.RegisterType(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            container.RegisterType<IUnitOfWork, UnitOfWork>();
            container.RegisterType<IContextService, ContextService>();
            container.RegisterType<IConfigService, ConfigService>();
            container.RegisterType<IHashingService, HashingService>();
            container.RegisterType<IParticipantService, ParticipantService>();
            container.RegisterType<IWebJobParticipantService, WebJobParticipantService>();
            container.RegisterType<IDashboardService, DashboardService>();
            container.RegisterType<IExportService, ExportService>();
            container.RegisterType<ISiteService, SiteService>();
            container.RegisterType<IReportService, ReportService>();
            container.RegisterType<IResponseService, ResponseService>();
            container.RegisterType<ICRAService, CRAService>();
            container.RegisterType<IPROCAS2UserManager, PROCASUserManager>();
            container.RegisterType<IWebJobLogger, WebJobLogger>();
            container.RegisterType<IHistologyService, HistologyService>();

            container.RegisterType<DbContext, ApplicationDbContext>(new HierarchicalLifetimeManager());
            

            container.RegisterType<ApplicationUserManager>(new HierarchicalLifetimeManager());
            


            container.RegisterType<ApplicationSignInManager>(new HierarchicalLifetimeManager());


            container.RegisterType<IAuthenticationManager>(
                    new InjectionFactory(c => HttpContext.Current.GetOwinContext().Authentication));

            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(
                new InjectionConstructor(typeof(ApplicationDbContext)));


           


            //Identity / Unity stuff below to fix No IUserToken Issue  - http://stackoverflow.com/questions/24731426/register-iauthenticationmanager-with-unity
            //container.RegisterType<DbContext, ApplicationDbContext>(
            //    new HierarchicalLifetimeManager());
            container.RegisterType<UserManager<ApplicationUser>>(
                new HierarchicalLifetimeManager());
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(
                new HierarchicalLifetimeManager());


            container.RegisterType<PROCAS2Context>(new PerResolveLifetimeManager());




            DependencyResolver.SetResolver(new Unity.Mvc5.UnityDependencyResolver(container));

            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);

            
        }
    }
}