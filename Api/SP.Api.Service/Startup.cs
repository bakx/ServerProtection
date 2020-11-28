using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace SP.Api.Service
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
#if DEBUG
			services.AddEntityFrameworkSqlServer().AddDbContext<Db>((sp, options) =>
			{
				options
					.UseSqlServer(Configuration.GetConnectionString("DevelopmentDatabase"))
					.UseInternalServiceProvider(sp);
			});
#else
            services.AddEntityFrameworkSqlServer().AddDbContext<Db>((sp, options) =>
            {
                options
                    .UseSqlServer(Configuration.GetConnectionString("ProductionDatabase"))
                    .UseInternalServiceProvider(sp);
            });
#endif

			services.AddGrpc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGrpcService<ApiService>();

				endpoints.MapGet("/",
					async context =>
					{
						await context.Response.WriteAsync(
							"Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
					});
			});
		}
	}
}