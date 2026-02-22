var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL with persistent data volume
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("octocare-postgres-data")
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("octocare");

// ASP.NET Core API
var api = builder.AddProject<Projects.Octocare_Api>("api")
    .WithReference(db)
    .WaitFor(db);

// Vite + React frontend
builder.AddViteApp("web", "../../web")
    .WithPnpm()
    .WithReference(api)
    .WithEnvironment("VITE_API_URL", api.GetEndpoint("https"));

builder.Build().Run();
