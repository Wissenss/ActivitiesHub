using EventsHub.Application.Core;
using EventsHub.Application.Core.Mapping;
using EventsHub.Application.Events.Queries;
using EventsHub.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventsHub.UnitTests;

[SetUpFixture]
public class GlobalTestSetup
{
    public static AppDbContext AppDbContext { get; private set; }
    public static ServiceProvider Services { get; private set; }

    [OneTimeSetUp]
    public async Task Setup()
    {
        DbContextOptions<AppDbContext> options = 
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data source=eventshub.db")
                .Options;
        AppDbContext = new AppDbContext(options);
        await AppDbContext.Database.MigrateAsync();
        await DbInitializer.SeedDataAsync(AppDbContext);

        // Same registrations as Program.cs, sharing the seeded context
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(AppDbContext);
        services.AddMediatR(opt => opt.RegisterServicesFromAssemblyContaining<GetEventList.Handler>());
        services.AddMapper(typeof(MappingProfiles).Assembly);
        Services = services.BuildServiceProvider();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await Services.DisposeAsync();
        await AppDbContext.DisposeAsync();
    }
}
