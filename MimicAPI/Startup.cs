using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimicAPI.Database;
using MimicAPI.Helpers;
using MimicAPI.V1.Repositories;
using MimicAPI.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using MimicAPI.Helpers.Swagger;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;

namespace MimicAPI
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
            services.AddControllers();
            services.AddMvc();
            services.AddScoped<IPalavraRepository, PalavraRepository>();
            services.AddApiVersioning(cfg => {
                cfg.ReportApiVersions = true;
                cfg.AssumeDefaultVersionWhenUnspecified = true;
                cfg.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            });

            services.AddSwaggerGen(cfg => {
                cfg.ResolveConflictingActions(apiDescription => apiDescription.First());
                cfg.SwaggerDoc("v2.0", new OpenApiInfo
                {
                    Title = "MimicAPI - V2.0",
                    Version = "v2.0"
                });
                cfg.SwaggerDoc("v1.1", new OpenApiInfo
                {
                    Title = "MimicAPI - V1.1",
                    Version = "v1.1"
                });
                cfg.SwaggerDoc("v1.0", new OpenApiInfo
                {
                    Title = "MimicAPI - V1.0",
                    Version = "v1.0"
                });

                var CaminhoProjeto = PlatformServices.Default.Application.ApplicationBasePath;
                var NomeProjeto = $"{ PlatformServices.Default.Application.ApplicationName}.xml";
                var CaminhoXML = Path.Combine(CaminhoProjeto, NomeProjeto);

                cfg.IncludeXmlComments(CaminhoXML);

                cfg.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();
                    // would mean this action is unversioned and should be included everywhere
                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }
                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
                    }
                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
                });

                cfg.OperationFilter<ApiVersionOperationFilter>();
            });

            services.AddDbContext<MimicContext>(opt => {
                opt.UseSqlite("Data Source=Database\\Mimic.db");
            });

            #region AutoMapper - Config
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DTOMapperProfile());
            });

            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePages();


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(cfg => {
                cfg.SwaggerEndpoint("/swagger/v2.0/swagger.json", "v2.0");
                cfg.SwaggerEndpoint("/swagger/v1.1/swagger.json", "v1.1");
                cfg.SwaggerEndpoint("/swagger/v1.0/swagger.json", "v1.0");
                cfg.RoutePrefix = String.Empty;

            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
