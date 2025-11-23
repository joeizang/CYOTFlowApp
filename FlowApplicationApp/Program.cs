using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlowApplicationApp.Data;
using FlowApplicationApp.Data.DomainModels;
using FlowApplicationApp.Models;

using dotenv.net;

var builder = WebApplication.CreateBuilder(args);
DotEnv.Load();
var env = DotEnv.Fluent().WithEnvFiles(".env").Read();
Console.WriteLine(env?["POSTGRES_CONN_STR"]);
var connectionString = env?["POSTGRES_CONN_STR"] ?? throw new InvalidOperationException("Connection string 'POSTGRES_CONN_STR' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
    .UseSeeding((context, _) =>
    {
      var roles = context.Set<FlowRoles>().Any();

      if(!roles)
        {
            context.Set<FlowRoles>().AddRange(SeedClass.PrepareRolesSeed());
            context.SaveChanges();
        }
    })
    .UseAsyncSeeding(async (context, _, token) =>
    {
      var roles = await context.Set<FlowRoles>().AnyAsync(token).ConfigureAwait(false);

      if(!roles)
        {
            context.Set<FlowRoles>().AddRange(SeedClass.PrepareRolesSeed());
            await context.SaveChangesAsync(token).ConfigureAwait(false);
        }
    }));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
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

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

await using (var scope = app.Services.CreateAsyncScope())
await using (var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
{
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();