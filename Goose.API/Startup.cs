using Goose.API.Authorization.Handlers;
using Goose.API.Repositories;
using Goose.API.Services;
using Goose.API.Services.Issues;
using Goose.API.Utils;
using Goose.Data;
using Goose.Data.Context;
using Goose.Data.Settings;
using Goose.Domain.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using Goose.API.Utils.Validators;

namespace Goose.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureMongoDB();
            RegisterService(services);
            ConfigureAuthorization(services);

            // Allows strings in the route parameter to be automatically be converted from strings.
            TypeDescriptor.AddAttributes(typeof(ObjectId), new TypeConverterAttribute(typeof(ObjectIdTypeConverter)));

            // Configure Cors
            services.AddCors(options =>
            {
                options.AddPolicy("cors",
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithOrigins(_configuration.GetSection("AllowedHosts").Get<string[]>());
                    });
            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection(nameof(TokenSettings)).Get<TokenSettings>().Secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

            services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new ObjectIdBinderProvider());
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new ObjectIdJsonConverter());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Goose.API", Version = "v1" });
                
                c.MapType<ObjectId>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "ObjectId",
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please add the 'Bearer' prefix. \r\n\r\n Example: Bearer eyJhbGciO...iJIUzI1NiIsInR5",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                        },
                        Array.Empty<string>()
                    }
                });

                // show comments in swagger
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        /// <summary>
        /// Use this method to register imperative authorization policies
        /// </summary>
        private void ConfigureAuthorization(IServiceCollection services)
        {
            
        }

        /// <summary>
        /// This method is used to configure the mongodb driver.
        /// </summary>
        private void ConfigureMongoDB()
        {
            // In order prevent the [BsonElement("...")] Attribute on each property we configure the drive to assume this as default. Thanks @LuksTrackmaniaCorner
            var conventionPack = new ConventionPack {new CamelCaseElementNameConvention(), new IgnoreExtraElementsConvention(true)};
            ConventionRegistry.Register("camelCase", conventionPack, t => true);
        }

        /// <summary>
        /// This method is extending the ConfigureServices method and allows to register classes for DI.
        /// </summary>
        /// <param name="services"></param>
        private void RegisterService(IServiceCollection services)
        {
            services.Configure<DbSettings>(_configuration.GetSection(nameof(DbSettings)));
            services.Configure<TokenSettings>(_configuration.GetSection(nameof(TokenSettings)));

            services.AddAutoMapper(typeof(AutoMapping));
            
            services.AddSingleton<IDbContext, DbContext>();
            services.AddScoped<IAuthorizationHandler, CompanyRoleHandler>();

            services.AddScoped<IIssueRepository, IssueRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();

            services.AddScoped<IIssueService, IssueService>();
            services.AddScoped<IIssueDetailedService, IssueDetailedService>();
            services.AddScoped<IIssueConversationService, IssueConversationService>();
            services.AddScoped<IIssueAssignedUserService, IssueAssignedUserService>();
            services.AddScoped<IIssueRequirementService, IssueRequirementService>();
            services.AddScoped<IIssueRequestValidator, IssueRequestValidator>();
            services.AddScoped<IIssuePredecessorService, IssuePredecessorService>();
            services.AddScoped<IIssueTimeSheetService, IssueTimeSheetService>(); 
            services.AddScoped<IUserService, UserService>();     
            services.AddScoped<IRoleService, RoleService>();    
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IProjectUserService, ProjectUserService>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICompanyUserService, CompanyUserService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddHttpContextAccessor();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/error");

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Goose.API v1"));

            //! https will not be used for this project, one the one side it adds complexity and the server is only accessable via ip and an certificate cannot be applied without domain name.
            //app.UseHttpsRedirection();

            app.UseCors("cors");

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapControllers().RequireAuthorization(); // enforce jwt token validation on all controllers...
            });
        }
    }
}