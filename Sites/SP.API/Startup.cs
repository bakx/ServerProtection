using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SP.API
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
			services.AddResponseCaching();
			services.AddHttpContextAccessor();
			services.AddControllers();

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
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseResponseCaching();

			app.UseAuthorization();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}