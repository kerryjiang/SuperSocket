using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AsSuperSocketHostBuilder<TextPackageInfo>()
    .UsePipelineFilter<LinePipelineFilter>()
    .UsePackageHandler(async (s, p) =>
    {
        // echo message back to client
        await s.SendAsync(Encoding.UTF8.GetBytes(p.Text + "\r\n"));
    })
    .UseInProcSessionContainer()
    .AsMinimalApiHostBuilder()
    .ConfigureHostBuilder();

var app = builder.Build();

app.MapGet("api/session", ([FromServices]ISessionContainer sessions) =>Microsoft.AspNetCore.Http.Results.Json(sessions.GetSessions()));

await app.RunAsync();