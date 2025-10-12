using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace LiveChat
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
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Serve Angular static files from ClientApp/dist
            var angularPath = Path.Combine(env.ContentRootPath, "ClientApp", "dist");
            if (Directory.Exists(angularPath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(angularPath),
                    RequestPath = ""
                });
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                // Fallback for Angular SPA routing
                endpoints.MapFallback(async context =>
                {
                    var indexPath = Path.Combine(angularPath, "index.html");
                    if (File.Exists(indexPath))
                    {
                        context.Response.ContentType = "text/html";
                        var indexContent = await File.ReadAllBytesAsync(indexPath);
                        await context.Response.Body.WriteAsync(indexContent);
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                    }
                });
            });
        }
    }
}
