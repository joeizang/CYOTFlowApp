using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlowApplicationApp.Data;
using FlowApplicationApp.Data.DomainModels;
using FlowApplicationApp.Models;

using dotenv.net;
using FluentValidation;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
DotEnv.Load();
var env = DotEnv.Fluent().WithEnvFiles(".env").Read();

var connectionString = env?["SQLITE_CONN_STR"] ?? throw new InvalidOperationException("Connection string 'SQLITE_CONN_STR' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString)
    );
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<FlowMember>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<FlowRoles>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});
builder.Services.AddControllersWithViews();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddScoped<UserManager<FlowMember>>();
builder.Services.AddScoped<RoleManager<FlowRoles>>();
// builder.Services.AddScoped<SeedUsersSpecial>();

// Register document conversion services
builder.Services.AddScoped<FlowApplicationApp.Infrastructure.Services.IDocumentConversionService, FlowApplicationApp.Infrastructure.Services.DocumentConversionService>();
builder.Services.AddScoped<FlowApplicationApp.Infrastructure.Services.ICodeOfConductService, FlowApplicationApp.Infrastructure.Services.CodeOfConductService>();

builder.Services.Configure<IdentityOptions>(opt =>
{
   opt.Password.RequireDigit = true;
   opt.Password.RequireLowercase = true;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 8;
    opt.Password.RequiredUniqueChars = 1;
    // Lockout settings.
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    opt.Lockout.MaxFailedAccessAttempts = 10;
    opt.Lockout.AllowedForNewUsers = true;

    // User settings.
    opt.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    opt.User.RequireUniqueEmail = true;

});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(System.Net.IPAddress.Parse("127.0.0.1"));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<FlowMember>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<FlowRoles>>();
    
        if (!await roleManager.Roles.AnyAsync())
        {
            foreach (var role in SeedClass.PrepareRolesSeed())
            {
                await roleManager.CreateAsync(role);
            }    
        }

    foreach (var member in SeedClass.PrepareAdminMemberSeed())
    {
        var user = await userManager.FindByEmailAsync(member?.Email);
        if (user == null)
        {
            var createdUser = await userManager.CreateAsync(member, env?["ADMIN_PASSWORD"] ?? "John832=>2026");
            if (createdUser.Succeeded)
            {
                await userManager.AddToRoleAsync(member, "Admin");
            }
        }        
    }
}

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
var envr = app.Environment;
// var properPath = Path.Combine(envr.ContentRootPath, "../../uploadFiles");
var properPath = Path.GetFullPath(Path.Combine(envr.ContentRootPath, "../../uploadFiles"));

if(!Directory.Exists(properPath))
{
    Directory.CreateDirectory(properPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(properPath),
    RequestPath = "/uploads"

});
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

    // .UseSeeding((context, _) =>
    // {
    //     var roles = context.Set<FlowRoles>().Any();
    //
    //     if(!roles)
    //     {
    //         context.Set<FlowRoles>().AddRange(SeedClass.PrepareRolesSeed());
    //         context.SaveChanges();
    //     }
    //
    //     if(!context.Set<FlowMember>().Any(m => m.Roles.Any(r => r.RoleName == "Admin")))
    //     {
    //         context.Set<FlowMember>().AddRange(SeedClass.PrepareAdminMemberSeed());
    //         context.SaveChanges();
    //     }
    // })
    // .UseAsyncSeeding(async (context, _, token) =>
    // {
    //     var roles = await context.Set<FlowRoles>().AnyAsync(token).ConfigureAwait(false);
    //
    //     if(!roles)
    //     {
    //         context.Set<FlowRoles>().AddRange(SeedClass.PrepareRolesSeed());
    //         await context.SaveChangesAsync(token).ConfigureAwait(false);
    //     }
    //     if(!context.Set<FlowMember>().Any(m => m.Roles.Any(r => r.RoleName == "Admin")))
    //     {
    //         context.Set<FlowMember>().AddRange(SeedClass.PrepareAdminMemberSeed());
    //         context.SaveChanges();
    //     }
    // })