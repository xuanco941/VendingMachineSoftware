using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using StyleX;
using StyleX.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 300;
});

builder.Services.Configure<FormOptions>(options =>
{
    //300MB
    options.MultipartBodyLengthLimit = 1024 * 1024 * 300;
});

//auth 
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(Common.CookieAuthUser, option =>
{
    option.LoginPath = "/access/login";
    option.LogoutPath = "/access/logout";
    option.ExpireTimeSpan = TimeSpan.FromDays(30);
}).AddCookie(Common.CookieAuthAdmin, option =>
{
    option.LoginPath = "/AdminAccess/login";
    option.LogoutPath = "/AdminAccess/logout";
    option.ExpireTimeSpan = TimeSpan.FromDays(30);
});


//service db 
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure();
    }));// shorthand getSection("ConnectionStrings")["DefaultConnection"]

builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

//http context
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);



var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


//get static file .glb
StaticFileOptions options = new StaticFileOptions { ContentTypeProvider = new FileExtensionContentTypeProvider() };
options.ServeUnknownFileTypes = true;
app.UseStaticFiles(options);

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");


app.Run();
