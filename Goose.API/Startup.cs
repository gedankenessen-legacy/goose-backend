using Goose.API.Repositories;
using Goose.API.Services;
using Goose.API.Services.issues;
using Goose.API.Utils;
using Goose.Data;
using Goose.Data.Context;
using Goose.Data.Settings;
using Goose.Domain.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using System.ComponentModel;

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
            });
        }

        /// <summary>
        /// This method is used to configure the mongodb driver.
        /// </summary>
        private void ConfigureMongoDB()
        {
            // In order prevent the [BsonElement("...")] Attribute on each property we configure the drive to assume this as default. Thanks @LuksTrackmaniaCorner
            var conventionPack = new ConventionPack { new CamelCaseElementNameConvention(), new IgnoreExtraElementsConvention(true) };
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

            services.AddScoped<IIssueRepository, IssueRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();

            services.AddScoped<IIssueService, IssueService>();
            services.AddScoped<IIssueConversationService, IssueConversationService>();
            services.AddScoped<IIssueAssignedUserService, IssueAssignedUserService>();
            services.AddScoped<IIssueRequirementService, IssueRequirementService>();
            services.AddScoped<IIssuePredecessorService, IssuePredecessorService>();
            services.AddScoped<IIssueTimeSheetService, IssueTimeSheetService>();       
            services.AddScoped<IUserService, UserService>();     
            services.AddScoped<IRoleService, RoleService>();    
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IProjectUserService, ProjectUserService>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<ICompanyService, CompanyService>();
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
