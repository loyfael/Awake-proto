
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeamAwake.Protocol.Security;
using TeamAwake.Servers.RealmServer.Database;
using TeamAwake.Servers.RealmServer.Database.Repositories;
using TeamAwake.Servers.RealmServer.Handlers.Authentication;
using TeamAwake.Servers.RealmServer.Network;
using Serilog;
using TeamAwake.Core.Network.Dispatchers;
using TeamAwake.Core.Network.Factories;
using TeamAwake.Core.Network.Options;
using TeamAwake.Core.Network.Parsers;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .Build();

var services = new ServiceCollection()
    .AddSingleton<IConfiguration>(config)
    .Configure<TransportServerOptions>(config.GetSection("Network"))
    .AddLogging(x => x.ClearProviders().AddSerilog(dispose: true))
    .AddSingleton<IMessageParser, MessageParser>()
    .AddSingleton<IMessageFactory, MessageFactory>()
    .AddScoped<IMessageParser, MessageParser>()
    .AddScoped<IMessageDispatcher, MessageDispatcher>()
    .AddScoped<IAccountRepository, AccountRepository>()
    .AddSingleton<RealmServer>()
    .AddSingleton<AuthenticationHandler>()
    .AddDbContext<RealmDbContext>(
        options =>
        {
            options.EnableDetailedErrors();
            
            options.UseNpgsql(config.GetConnectionString("Postgres"), npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("migrations");
                npgsqlOptions.MigrationsAssembly(typeof(RealmSession).Assembly.FullName);
            });
        })
    .BuildServiceProvider();

var messageFactory = services.GetRequiredService<IMessageFactory>();
var messageHandler = services.GetRequiredService<IMessageDispatcher>();

messageFactory.RegisterMessages(typeof(IdentificationMessage).Assembly);
messageHandler.RegisterHandlers(typeof(RealmSession).Assembly);

await services.GetRequiredService<RealmServer>().StartAsync();
    