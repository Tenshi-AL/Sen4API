using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Sen4.Authorizations;
using Sen4.Helpers;
using Sen4.Hubs;
using Sen4.Middlewares;
using Sen4.ServiceExtensions;
using Sen4.SignalR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//logger configuration
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddTransient<ExtensionHandlerMiddleware>();
builder.Services.AddTransient<LogRequestMiddleware>();
builder.Services.AddMemoryCache();

builder.Services.AddControllers(options =>
    {
        options.InputFormatters.Insert(0, MyJPIF.GetJsonPatchInputFormatter());
    })
    .AddFluentValidation(config =>
        config.RegisterValidatorsFromAssembly(typeof(ProjectValidator).Assembly));

builder.Services.SwaggerConfigure();
builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddDbContext<Sen4Context>(p => p.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.IdentityConfigure();


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectTaskService, ProjectTaskService>();
builder.Services.AddScoped<ITaskStatusService, TaskStatusService>();
builder.Services.AddScoped<IPriorityService, PriorityService>();
builder.Services.AddScoped<IRuleService, RuleService>();
builder.Services.AddScoped<IOperationService, OperationService>();
builder.Services.AddScoped<ISen4AuthService, AuthenticationService>();


builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuthorizationHandler, AuthorizationHandler>();

builder.Services.AuthenticationConfigure(builder.Configuration);

builder.Services.AddSingleton<IUserIdProvider, IdProvider>(); 

builder.Services.AddSignalR();

builder.Services.AddMinIOConfiguration(builder.Configuration);

var app = builder.Build();
app.UseMiddleware<LogRequestMiddleware>();
app.UseMiddleware<ExtensionHandlerMiddleware>();
app.UseCors(corsPolicyBuilder => corsPolicyBuilder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("Content-Disposition")); // Указываем, что заголовок Content-Disposition доступен для клиента;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<NotificationHub>("/notification");
app.Run();

public partial class Program { }