using Microsoft.EntityFrameworkCore;
using SampleWebApp.Data;
using SampleWebApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure the database
var connectionString = builder.Configuration.GetConnectionString("Todos");
builder.Services.AddSqlite<ApplicationDbContext>(connectionString);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    // Seed initial data if needed
    if (!context.Todos.Any())
    {
        // Seed your initial data here
        var initialTodos = new List<Todo>
        {
            new Todo { Id = 1, Title = "Created Sample App", IsComplete = true },
            new Todo { Id = 2, Title = "Tested Open Telemetry", IsComplete = true },
            new Todo { Id = 3, Title = "Profit?", IsComplete = false }
        };

        context.Todos.AddRange(initialTodos);
        context.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
