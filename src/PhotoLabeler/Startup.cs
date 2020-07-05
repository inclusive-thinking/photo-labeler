// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Data;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotoLabeler.Data.Interfaces.Repositories;
using PhotoLabeler.Data.Repositories.Interfaces;
using PhotoLabeler.Interfaces;
using PhotoLabeler.PhotoStorageReader.Implementations;
using PhotoLabeler.PhotoStorageReader.Interfaces;
using PhotoLabeler.ServiceLibrary.Implementations;
using PhotoLabeler.ServiceLibrary.Interfaces;
using PhotoLabeler.Services;

namespace PhotoLabeler
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = (IConfigurationRoot)configuration;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var supportedCultures = new[]
            {
                "en",
                "es"
            };

            var opt = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),
            };
            opt.AddSupportedCultures(supportedCultures);
            opt.AddSupportedUICultures(supportedCultures);
            services.AddSingleton(opt);
            services.AddMvc();
            services.AddRazorPages();
            services.AddServerSideBlazor(opt => opt.DetailedErrors = true);
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            services.AddSingleton<IPhotoLabelerService, PhotoLabelerService>();
            services.AddSingleton<IPhotoReader, PhotoReaderBase64>();        
            services.AddSingleton<IMenuService, MenuService>();
            services.AddSingleton<IDbConnection>((serviceProvider) =>
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "./appconfig.db" };
                var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
                return connection;
            });
            services.AddSingleton<IAppConfigRepository, AppConfigRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RequestLocalizationOptions localizationOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseRequestLocalization(localizationOptions);
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
            Electron.App.CommandLine.AppendSwitch("disable -http-cache");

            //var browserWindowOptions = new BrowserWindowOptions
            //{
            //    WebPreferences = new WebPreferences
            //    {
            //        WebSecurity = false,
            //    },
            //};

            Task.Run(async () => await Electron.WindowManager.CreateWindowAsync(
				//browserWindowOptions,
                $"http://localhost:{BridgeSettings.WebPort}/Language/SetCultureByConfig"
            ));
        }
    }
}
