using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Diagnostics.Entity;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Routing;
using Microsoft.Data.Entity;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
using Microsoft.Framework.Runtime;
using CLS.Models;
using System.Security.Claims;
using Microsoft.AspNet.SignalR;

namespace CLS
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Setup configuration sources.
            var configuration = new Configuration()
                .AddJsonFile("config.json")
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
            {
                // This reads the configuration keys from the secret store.
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                configuration.AddUserSecrets();
            }
            configuration.AddEnvironmentVariables();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Application settings to the services container.
            services.Configure<AppSettings>(Configuration.GetSubKey("AppSettings"));

            // Add EF services to the services container.
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            // Add Identity services to the services container.
            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequiredLength = 4;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonLetterOrDigit = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add MVC services to the services container.
            services.AddMvc();

            // Add SignalR
            services.AddSignalR(opt => 
            {
                opt.Hubs.EnableDetailedErrors = true;
            }
            );

            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();


        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            // Configure the HTTP request pipeline.

            // Add the console logger.
            loggerfactory.AddConsole(minLevel: LogLevel.Warning);

            // Add the following to the request pipeline only in development environment.
            if (env.IsEnvironment("Development"))
            {
                app.UseBrowserLink();
                app.UseErrorPage(ErrorPageOptions.ShowAll);
                app.UseDatabaseErrorPage(DatabaseErrorPageOptions.ShowAll);
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // sends the request to the following path or controller action.
                app.UseErrorHandler("/Home/Error");
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline.
            app.UseIdentity();

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });

            // Add SignalR to the request pipeline.
            app.UseSignalR();

            CreateSampleUsers(app.ApplicationServices).Wait();
            CreateTestData();

            

        }
        private static async Task CreateSampleUsers(IServiceProvider applicationServices)
        {
            // currently the users are static! Change if necessary... then we also need a UserManagement-Tool!
            // User Data is also stored in cockies... we need to check this a little bit more!
            // add users
            var userManager = applicationServices.GetService<UserManager<ApplicationUser>>();

            var usr = new ApplicationUser { UserName = "Guest" };

            var result = await userManager.CreateAsync(usr, "0000");
            await userManager.AddClaimAsync(usr, new Claim("CanShowMainView", "true"));

            usr = new ApplicationUser { UserName = "Operator" };

            result = await userManager.CreateAsync(usr, "0000");
            await userManager.AddClaimAsync(usr, new Claim("CanShowMainView", "true"));
            await userManager.AddClaimAsync(usr, new Claim("CanShowContextMenu", "true"));

            usr = new ApplicationUser { UserName = "Admin" };

            result = await userManager.CreateAsync(usr, "6883");
            await userManager.AddClaimAsync(usr, new Claim("CanShowMainView", "true"));
            await userManager.AddClaimAsync(usr, new Claim("CanShowContextMenu", "true"));
            await userManager.AddClaimAsync(usr, new Claim("CanLockContainerPlaces", "true"));

        }

        private void CreateTestData()
        {
            CLSModel.ContainerInAir = new Container() { Id = "20ft/corners NY-Wast-Container 1" };

            CLSModel.Containers.Add(new Container() { Id = "20ft/corners NY-Wast-Container 1" });
            CLSModel.Containers.Add(new Container() { Id = "20ft/corners NY-Wast-Container 2" });
            CLSModel.Containers.Add(new Container() { Id = "20ft/corners NY-Wast-Container 3" });
            CLSModel.Containers.Add(new Container() { Id = "20ft/corners NY-Wast-Container 4" });
            CLSModel.Containers.Add(new Container() { Id = "20ft/corners NY-Wast-Container 5" });

            CLSModel.TransferCarPlaces.Add(new TransferCarPlace() { Position = new Position() { xPos = 1, yPos = 1 }, Id = "SC_D" });
            CLSModel.TransferCarPlaces.Add(new TransferCarPlace() { Position = new Position() { xPos = 2, yPos = 1 }, Id = "SC_C", Container = new Container() { Id = "20ft/corners NY-Wast-" } });
            CLSModel.TransferCarPlaces.Add(new TransferCarPlace() { Position = new Position() { xPos = 3, yPos = 1 }, Id = "SC_B", Container = new Container() { Id = "20ft/corners NY-Wast-Container 3" } });
            CLSModel.TransferCarPlaces.Add(new TransferCarPlace() { Position = new Position() { xPos = 4, yPos = 1 }, Id = "SC_A", Container = new Container() { Id = "20ft/corners NY-Wast-Container 4" } });

            CLSModel.CranePlaces.Add(new CranePlace() { Position = new Position() { xPos = 1, yPos = 1 }, Id = "Crane 1", ContainerPlaceType = "3", IsLocked = false });
            CLSModel.CranePlaces.Add(new CranePlace() { Position = new Position() { xPos = 2, yPos = 1 }, Id = "Crane 2", ContainerPlaceType = "3", IsLocked = true });

            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 1, yPos = 1 }, Id = "B01", IsLocked = true });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 1, yPos = 2 }, Id = "B02" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 1, yPos = 3 }, Id = "B03" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 1, yPos = 4 }, Id = "B04" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 2, yPos = 1 }, Id = "B05" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 2, yPos = 2 }, Id = "B06" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 2, yPos = 3 }, Id = "B07" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 2, yPos = 4 }, Id = "B08" });

            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 3, yPos = 1 }, Id = "TB1", IsLocked = true });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 3, yPos = 2 }, Id = "B09" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 3, yPos = 3 }, Id = "B10" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 3, yPos = 4 }, Id = "B11" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 4, yPos = 1 }, Id = "TB2" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 4, yPos = 2 }, Id = "B12" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 4, yPos = 3 }, Id = "B13" });
            CLSModel.StockPlaces.Add(new StockPlace() { Position = new Position() { xPos = 4, yPos = 4 }, Id = "B14" });



            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 1, yPos = 1 } });
            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 2, yPos = 1 } });
            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 3, yPos = 1 } });
            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 4, yPos = 1 } });
            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 5, yPos = 1 } });
            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 6, yPos = 1 } });

            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 1, yPos = 2 } });
            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 2, yPos = 2 } });
            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 3, yPos = 2 } });
            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 4, yPos = 2 } });
            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 5, yPos = 2 } });
            CLSModel.BargePlaces.Add(new ContainerPlace() { Position = new Position() { xPos = 6, yPos = 2 } });
        }
    }
}
