using Goose.API.Repositories;
using Goose.API.Services;
using Goose.Data;
using Goose.Data.Context;
using Goose.Data.Settings;
using Goose.Domain.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new ObjectIdConverter());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Goose.API", Version = "v1" });
            });
        }

        /// <summary>
        /// This method is used to configure the mongodb driver.
        /// </summary>
        private void ConfigureMongoDB()
        {
            // In order prevent the [BsonElement("...")] Attribute on each property we configure the drive to assume this as default. Thanks @LuksTrackmaniaCorner
            var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("camelCase", conventionPack, t => true);
        }

        /// <summary>
        /// This method is extending the ConfigureServices method and allows to register classes for DI.
        /// </summary>
        /// <param name="services"></param>
        private void RegisterService(IServiceCollection services)
        {
            services.Configure<DbSettings>(_configuration.GetSection(nameof(DbSettings)));

            services.AddTransient<IIssueRepository, IssueRepository>();
            services.AddTransient<IIssueConversationService, IssueConversationService>();

            services.AddSingleton<IDbContext, DbContext>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRoleService, RoleService>();

            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IProjectService, ProjectService>();

            services.AddScoped<IStateService, StateService>();

            services.AddAutoMapper(typeof(AutoMapping));
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

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
