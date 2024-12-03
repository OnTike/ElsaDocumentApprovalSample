using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;


// Add Elsa services.
services.AddElsa(elsa =>
{
    elsa.AddWorkflowsFrom<Program>();
    
    elsa.AddSwagger();
    //try this to reproduce the error (need postgres on <your vps or localhost>:5432 with Elsa DB)
    
    elsa.UseWorkflowManagement(management =>
            management.UseEntityFrameworkCore(x =>
                x.UsePostgreSql(configuration.GetConnectionString("Elsa")!)))
        .UseWorkflowRuntime(runtime =>
            runtime.UseEntityFrameworkCore(x =>
                x.UsePostgreSql(configuration.GetConnectionString("Elsa")!)));
    
    
    //The same error occurred in SqLite
    // elsa.UseWorkflowManagement(management =>
    //         management.UseEntityFrameworkCore(x =>
    //             x.UseSqlite()))
    //     .UseWorkflowRuntime(runtime =>
    //         runtime.UseEntityFrameworkCore(x =>
    //             x.UseSqlite()));
    
    elsa.UseWorkflowsApi();
    
    elsa.UseSasTokens();

    elsa.UseHttp(http =>
        http.ConfigureHttpOptions = options => { options.BaseUrl = new Uri("https://localhost:5001"); });
});


// Configure middleware pipeline.
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

// Expose Elsa APIs.
app.UseWorkflowsApi();

app.UseSwaggerUI();
// Add Elsa HTTP middleware (to handle requests mapped to HTTP Endpoint activities)
app.UseWorkflows();

// Start accepting requests.
app.Run();