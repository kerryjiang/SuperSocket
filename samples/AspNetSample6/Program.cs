using SuperSocket;
using SuperSocket.ProtoBase;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Host.AsSuperSocketHostBuilder<TextPackageInfo, LinePipelineFilter>()
            .UsePackageHandler(async (s, p) =>
            {
                // echo message back to client
                await s.SendAsync(Encoding.UTF8.GetBytes(p.Text + "\r\n"));
            })
            .UseInProcSessionContainer();

var app = builder.Build();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
