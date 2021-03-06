﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BlendedAdmin.DomainModel.Items;
using BlendedAdmin.DomainModel;
using BlendedAdmin.Data;
using Microsoft.EntityFrameworkCore;
using BlendedAdmin.Services;
using BlendedAdmin.DomainModel.Variables;
using Newtonsoft.Json.Serialization;
using BlendedAdmin.DomainModel.Environments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using BlendedAdmin.Js;
using BlendedAdmin.DomainModel.Users;
using Microsoft.AspNetCore.Identity;
using BlendedAdmin.Infrastructure;
using BlendedJS;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Rewrite;
using BlendedAdmin.DomainModel.Tenants;

namespace BlendedAdmin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services
                .AddMvc(x => 
                {
                    x.Filters.Add<EnvironmentFilter>();
                    //x.Filters.Add<ValidateTenantFilter>();
                })
                .AddJsonOptions(x => x.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddDbContext<ApplicationDbContext>(options => {
                DatabaseOptions databaseOptions = Configuration.GetSection("Database").Get<DatabaseOptions>();
                if (databaseOptions.ConnectionProvider.SafeEquals("Sqlite"))
                    options.UseSqlite(databaseOptions.ConnectionString);
                if (databaseOptions.ConnectionProvider.SafeEquals("SqlServer"))
                    options.UseSqlServer(databaseOptions.ConnectionString);
                if (databaseOptions.ConnectionProvider.SafeEquals("MySQL"))
                    options.UseMySql(databaseOptions.ConnectionString);
                if (databaseOptions.ConnectionProvider.SafeEquals("PostgreSQL") || databaseOptions.ConnectionProvider.SafeEquals("Postgres"))
                    options.UseNpgsql(databaseOptions.ConnectionString);
            });
            services.AddTransient<IUserStore<ApplicationUser>, ApplicationUserStore>();
            services.AddTransient<IDomainContext, DomainContext>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IItemRepository, ItemRepository>();
            services.AddTransient<IVariableRepository, VariableRepository>();
            services.AddTransient<IEnvironmentRepository, EnvironmentRepository>();
            services.AddTransient<ITenantRepository, TenantRepository>();
            services.AddTransient<IEnvironmentService, EnvironmentService>();
            services.AddTransient<ISiteMenuService, SiteMenuService>();
            services.AddTransient<IUrlService, UrlService>();
            services.AddTransient<IVariablesService, VariablesService>();
            services.AddTransient<ITenantService, TenantService>();
            services.AddTransient<IJsService, JsService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/user/login";
                options.LogoutPath = "/user/logoff";
                options.AccessDeniedPath = "/accessdenied";
            });
            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(options => {
            //        options.LoginPath = "/{environment}/login";
            //        options.LogoutPath = "/{environment}/logoff";
            //        options.AccessDeniedPath = "/{environment}accessdenied";
            //    });
            services.AddOptions();
            services.Configure<DatabaseOptions>(Configuration.GetSection("Database"));
            services.Configure<HostingOptions>(Configuration.GetSection("Hosting"));
            services.Configure<MailOptions>(Configuration.GetSection("Mail"));
            services.Configure<SecurityOptions>(Configuration.GetSection("Security"));
            services.Configure<FileLoggerOptions>(Configuration.GetSection("Logging:File"));
            services.Configure<ElasticLoggerOptions>(Configuration.GetSection("Logging:Elastic"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvide, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var securityOptions = Configuration.GetSection("Security").Get<SecurityOptions>();
            if (securityOptions != null && securityOptions.EnforceHttps)
            {
                var options = new RewriteOptions()
                    .AddRedirectToProxiedHttps();
                    //.AddRedirect("(.*)/$", "$1");  // remove trailing slash

                app.UseRewriter(options);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseTenant();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{environment=Default}/{controller=Home}/{action=Index}/{id?}");
            });

            using (var scope = serviceProvide.CreateScope())
            {
                using (ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    dbContext.Database.Migrate();
                    if (dbContext.Users.Any(x => x.NormalizedUserName == "ADMIN") == false)
                    {
                        ApplicationUser admin = new ApplicationUser();
                        admin.UserName = "Admin";
                        admin.NormalizedUserName = "ADMIN";
                        admin.PasswordHash = serviceProvide.GetService<IPasswordHasher<ApplicationUser>>().HashPassword(admin, "admin");
                        admin.SecurityStamp = Guid.NewGuid().ToString();
                        dbContext.Users.Add(admin);
                        dbContext.SaveChanges();
                    }
                }
            }
        }
    }
}
