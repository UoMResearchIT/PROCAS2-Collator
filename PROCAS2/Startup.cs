using Microsoft.Owin;
using Owin;

using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

using PROCAS2.Data.Identity;

[assembly: OwinStartupAttribute(typeof(PROCAS2.Startup))]
namespace PROCAS2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createRolesandUsers();
        }

        // In this method we will create default User roles and Admin user for login   
        private void createRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


            // Create Admin   
            if (!roleManager.RoleExists("Super"))
            {

                // first we create Admin rool   
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Super";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                //var user = new ApplicationUser();
                //user.UserName = "shanu";
                //user.Email = "syedshanumcain@gmail.com";

                //string userPWD = "A@Z200711";

                //var chkUser = UserManager.Create(user, userPWD);

                ////Add default User to Role Admin   
                //if (chkUser.Succeeded)
                //{
                //    var result1 = UserManager.AddToRole(user.Id, "Admin");

                //}
            }

            // creating Creating General role    
            if (!roleManager.RoleExists("General"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "General";
                roleManager.Create(role);

            }
        }
    }
}
