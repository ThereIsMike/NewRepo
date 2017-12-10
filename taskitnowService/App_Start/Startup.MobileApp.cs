using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using taskitnowService.DataObjects;
using taskitnowService.Models;
using Owin;

namespace taskitnowService
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            //For more information on Web API tracing, see http://go.microsoft.com/fwlink/?LinkId=620686 
            config.EnableSystemDiagnosticsTracing();

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            // Use Entity Framework Code First to create database tables based on your DbContext
            Database.SetInitializer(new taskitnowInitializer());

            // To prevent Entity Framework from modifying your database schema, use a null database initializer
            // Database.SetInitializer<taskitnowContext>(null);

            MobileAppSettingsDictionary settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();

            if (string.IsNullOrEmpty(settings.HostName))
            {
                // This middleware is intended to be used locally for debugging. By default, HostName will
                // only have a value when running in an App Service application.
                app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions
                {
                    SigningKey = ConfigurationManager.AppSettings["SigningKey"],
                    ValidAudiences = new[] { ConfigurationManager.AppSettings["ValidAudience"] },
                    ValidIssuers = new[] { ConfigurationManager.AppSettings["ValidIssuer"] },
                    TokenHandler = config.GetAppServiceTokenHandler()
                });
            }
            app.UseWebApi(config);
        }
    }

    public class taskitnowInitializer : CreateDatabaseIfNotExists<taskitnowContext>
    {
        protected override void Seed(taskitnowContext context)
        {
            List<TodoItem> todoItems = new List<TodoItem>
            {
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "First item", Complete = false },
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "Second item", Complete = false },
            };

            foreach (TodoItem todoItem in todoItems)
            {
                context.Set<TodoItem>().Add(todoItem);
            }
            List<Item> manyproducts = new List<Item>
            {
                new Item { Id = Guid.NewGuid().ToString(), Name = "Milk" },
                new Item { Id = Guid.NewGuid().ToString(), Name = "Coffee" },
            };

            foreach (Item product in manyproducts)
            {
                context.Set<Item>().Add(product);
            }

            List<User> manyusers = new List<User>
            {
                new User { Id = Guid.NewGuid().ToString(), FirstName = "Anna", SecondName = "K" },
                new User { Id = Guid.NewGuid().ToString(), FirstName = "Mike",  SecondName = "K"},
            };

            foreach (User user in manyusers)
            {
                context.Set<User>().Add(user);
            }

            base.Seed(context);
        }
    }
}

