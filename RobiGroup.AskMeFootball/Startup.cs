using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using RobiGroup.AskMeFootball.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using RobiGroup.AskMeFootball.Common.Localization;
using RobiGroup.AskMeFootball.Common.Options;
using RobiGroup.AskMeFootball.Core.Game;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Core.Identity;
using RobiGroup.Web.Common;
using RobiGroup.Web.Common.Binders;
using RobiGroup.Web.Common.Configuration;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Localizer;
using RobiGroup.Web.Common.Services;
using Swashbuckle.AspNetCore.Swagger;
using WebSocketManager;

namespace RobiGroup.AskMeFootball
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
            services.AddOptions();
            services.Configure<TokenProviderOptions>(Configuration.GetSection("TokenProviderOptions"));
            services.Configure<DefaultsOptions>(Configuration.GetSection("Defaults"));
            services.Configure<MobizonOptions>(Configuration.GetSection("Mobizon"));
            services.Configure<MatchOptions>(Configuration.GetSection("MatchOptions"));

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));


            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                //config.SignIn.RequireConfirmedPhoneNumber = true;

                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // Lockout settings
                // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                // options.Lockout.MaxFailedAccessAttempts = 10;
                // User settings
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedEmail = false;

            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddPhoneNumber4DigitTokenProvider();

            services.AddScoped<IAuthService<ApplicationUser>, AuthService<ApplicationUser>>();
            services.AddTransient<ISmsSender, MobizonSmsSender>();
            services.AddSingleton<IStringLocalizerFactory, ApplicationStringLocalizerFactory<Resources>>();

            services.AddSingleton<IMatchManager, MatchManager>();

            services.AddHostedService<GameTimerService>();

            var providerOptions = services.BuildServiceProvider().GetService<IOptions<TokenProviderOptions>>();
            services.AddAuthentication()
                .AddJwtBearer(x =>
                {
                    x.Audience = providerOptions.Value.Audience;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidateIssuer = true,
                        ValidIssuer = providerOptions.Value.Issuer,
                        IssuerSigningKey = providerOptions.Value.SigningCredentials.Key,
                    };
                    x.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["token"];

                            if (!string.IsNullOrEmpty(accessToken) &&
                                (context.HttpContext.WebSockets.IsWebSocketRequest || context.Request.Headers["Accept"] == "text/event-stream"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                    x.SaveToken = true;
                });

            services.AddAuthorization();

            services.AddMvc(o =>
            {
                o.ModelBinderProviders.Insert(0, new InvariantDecimalModelBinderProvider());
                var stringLocalizerFactory = (IStringLocalizerFactory)services.BuildServiceProvider()
                    .GetService(typeof(IStringLocalizerFactory));
                o.ModelMetadataDetailsProviders.Add(new ApplicationDisplayMetadataProvider<Resources>(stringLocalizerFactory));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaults = services.BuildServiceProvider().GetService<IOptions<DefaultsOptions>>();
                var supportedCultures = defaults.Value.Languages.Select(l => new CultureInfo(l)).ToArray();
                var defaultUiCulture = defaults.Value.Languages.First();
                options.DefaultRequestCulture = new RequestCulture(culture: defaultUiCulture, uiCulture: defaultUiCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.AddWebSocketManager();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "AskMeFootball API", Version = "v1" });

                //Set the comments path for the swagger json and ui.
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, @"RobiGroup.AskMeFootball.xml");
                c.IncludeXmlComments(xmlPath);

                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(security);

                //c.AddSecurityDefinition("authorization", new OAuth2Scheme()
                //{
                //    Description = "Authorization",
                //    Type = "oauth2",
                //    Flow = "clientcredentials",
                //    AuthorizationUrl = Configuration["OpenId:authority"],
                //    TokenUrl = Configuration["OpenId:authority"] + "/token",
                //    Scopes = new Dictionary<string, string>() {{"yourapi", "your api resources"}}
                //});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider,
            RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseCookiePolicy();



            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>()
                .Value);

            app.UseAuthentication();

            app.UseWebSockets();
            app.MapWebSocketManager("/game", serviceProvider.GetService<GamersHandler>(), JwtBearerDefaults.AuthenticationScheme);

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                var hostingEnvironment = serviceProvider.GetService<IHostingEnvironment>();

                c.IndexStream = () => new FileStream($"{hostingEnvironment.WebRootPath}/Swagger.index.html", FileMode.Open);

                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AskMeFootball API");
                c.RoutePrefix = "swagger";

                c.InjectJavascript("/js/WebSocketManager.js");
                c.InjectJavascript("/js/swagger.websocket.js");
            });

            roleManager.AddRolesToDbIfNotExists(ApplicationRoles.Roles);

            var dbContext = serviceProvider.GetService<ApplicationDbContext>();

            dbContext.Database.Migrate();

            if (!dbContext.Cards.Any())
            {
                var card = new Card
                {
                    Name = "Premier League",
                    MatchQuestions = 10,
                    Prize = "2000 KZT",
                    TypeId = 10,
                    ResetPeriod = 1,
                    ResetTime = DateTime.Now.AddDays(1)
                };

                dbContext.Cards.Add(card);
                dbContext.SaveChanges();

                for (int i = 0; i < 1000; i++)
                {
                    var question = new Question
                    {
                        CardId = card.Id,
                        Text = "Question " + (i + 1),
                    };
                    dbContext.Questions.Add(question);
                }

                dbContext.SaveChanges();

                foreach (var question in dbContext.Questions)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        dbContext.QuestionAnswers.Add(new QuestionAnswer
                        {
                            Text = "Answer " + (i+1),
                            QuestionId = question.Id
                        });
                    }
                }

                dbContext.SaveChanges();

                foreach (var question in dbContext.Questions.Include(q => q.Answers))
                {
                    question.CorrectAnswerId = question.Answers.OrderBy(a => a.Id).First().Id;
                }

                dbContext.SaveChanges();
            }
        }
    }
}
