using Funq;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.Razor;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using Website.Infrustructure.Authentication;
using Website.Infrustructure.DataSource;
using Website.Infrustructure.Manager;
using Website.Infrustructure.Models;

namespace Website
{
    public interface IAppConfig
    {
        bool BuildMode { get; set; }
    }

    //Hold App wide configuration you want to accessible by your services
    public class AppConfig : IAppConfig
    {
        public AppConfig(IResourceManager appSettings)
        {
            this.BuildMode = appSettings.Get("BuildDatabase", false);
        }
        public bool BuildMode { get; set; }
    }

    public class AppHost : AppHostBase
    {
        public AppHost() : base("Test Razor", typeof(AppHost).Assembly) { }

        public static AppConfig Config;

        public override void Configure(Container container)
        {
            LogManager.LogFactory = new ConsoleLogFactory();
            var appSettings = new AppSettings();
            Config = new AppConfig(appSettings);

            Plugins.Add(new RazorFormat());

            // Use an in-memory SQL Lite database:
            // container.Register<IDbConnectionFactory>(new OrmLiteConnectionFactory(":memory:", false, SqliteDialect.Provider));
            // Use SQL Server / SQL Azure database:
            container.Register<IDbConnectionFactory>(new OrmLiteConnectionFactory(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), SqlServerOrmLiteDialectProvider.Instance) { });
            
            container.Register<ICacheClient>(new MemoryCacheClient());
            container.Register<IAppConfig>(Config);
            container.Register<IRepository>(c => new Repository(c.Resolve<IDbConnectionFactory>()));
            container.Register<IUserAuthRepository>(c => new OrmLiteAuthRepository(c.Resolve<IDbConnectionFactory>()));
            container.Register<IStorageManager>(c => new StorageManager(c.Resolve<IRepository>()));

            // Factory default provided routes:
            // /auth/credentials                        <-- HTML Form login (using: CredentialsAuthProvider)
            // /auth/logout

            this.RequestFilters.Add((req, res, dto) =>
            {
                CustomUserSession.AutoSignIn(ref container, ref req, ref res);
                CustomUserSession.ApiSignIn(ref container, ref req, ref res);
            });

            Plugins.Add(new AuthFeature(() =>
                new CustomUserSession(),
                new IAuthProvider[] {
                new CredentialsAuthProvider(),         // HTML Form post of UserName/Password credentials
                new TwitterAuthProvider(appSettings),  // Sign-in with Twitter
                new FacebookAuthProvider(appSettings), // Sign-in with Facebook
                new CustomBasicAuthentication(),       // Basic authentication implementation
            }));
            // Provide service for new users to register so they can login with supplied credentials.
            // Removed: we will implement our own sign up form...
            // Plugins.Add(new RegistrationFeature());

            BuildDataSource(container);

            SetConfig(new EndpointHostConfig
            {
                CustomHttpHandlers = {
                    { HttpStatusCode.NotFound, new RazorHandler("/notfound") },
                    { HttpStatusCode.Unauthorized, new RazorHandler("/SignIn") },
                    { HttpStatusCode.ServiceUnavailable, new RazorHandler("/unavailable") }
                }
            });
        }

        private void BuildDataSource(Container container)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["BuildDatabase"].ToString()))
            {
                var authRepo = (OrmLiteAuthRepository)container.Resolve<IUserAuthRepository>();
                authRepo.DropAndReCreateTables();

                using (var db = container.Resolve<IDbConnectionFactory>().OpenDbConnection())
                {
                    db.DropTable<Asset>();
                    db.DropTable<User>();

                    db.CreateTable<User>();
                    db.CreateTable<Asset>();
                }

                // Add initial seed data:
                using (var db = container.Resolve<IDbConnectionFactory>().OpenDbConnection())
                {

                }
            }
        }
    }
}