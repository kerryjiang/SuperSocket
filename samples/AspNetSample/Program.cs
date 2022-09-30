using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
new SuperSocketHostBuilder<TextPackageInfo>(builder.Host)
    .UsePipelineFilter<LinePipelineFilter>()
    .UsePackageHandler(async (s, p) =>
    {
        // echo message back to client
        await s.SendAsync(Encoding.UTF8.GetBytes(p.Text + "\r\n"));
    })
    .UseInProcSessionContainer()
    .Build();

var app = builder.Build();

app.MapGet("api/session", ([FromServices]ISessionContainer sessions) =>Microsoft.AspNetCore.Http.Results.Json(sessions.GetSessions()));

await app.RunAsync();


public class SuperSocketHostBuilder<T> : SuperSocket.SuperSocketHostBuilder<T>
{
    public SuperSocketHostBuilder(IHostBuilder hostBuilder) : base(hostBuilder) { }
    public override IHost Build()
    {
        HostBuilder.ConfigureServices((ctx, services) =>
            {
                RegisterBasicServices(ctx, services, services);
            }).ConfigureServices((ctx, services) =>
            {
                foreach (var action in ConfigureServicesActions)
                {
                    action(ctx, services);
                }

                foreach (var action in ConfigureSupplementServicesActions)
                {
                    action(ctx, services);
                }
            }).ConfigureServices((ctx, services) =>
            {
                RegisterDefaultServices(ctx, services, services);
            });
        return null;
    }
}