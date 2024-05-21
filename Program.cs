
using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Models;
using AkilliFiyatWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPortfolyoWebSite.Entity;
using MyPortfolyoWebSite.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>(i =>
    new SmtpEmailSender(
        builder.Configuration["EmailSenderAkilliFiyat:Host"],
        builder.Configuration.GetValue<int>("EmailSenderAkilliFiyat:Port"),
        builder.Configuration.GetValue<bool>("EmailSenderAkilliFiyat:EnableSSL"),
        builder.Configuration["EmailSenderAkilliFiyat:Username"],
        builder.Configuration["EmailSenderAkilliFiyat:Password"])
);
// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<MyPortfolyoWebSite.Models.IdentityContext>(options =>
options.UseMySql(builder.Configuration["ConnectionStrings2:mysql_connection"], new MySqlServerVersion(new Version(8, 0, 30)))
);
builder.Services.AddDbContext<DataContext>(options =>
{
    var config = builder.Configuration;
    var connectionString = builder.Configuration["ConnectionStringsAkilliFiyat:mysql_connection"];
    //options.UseSqlite(connectionString);

    var version = new MySqlServerVersion(new Version(8, 0, 30));
    options.UseMySql(connectionString, version);
});

builder.Services.AddDbContext<AkilliFiyatWeb.Models.IdentityContext>(options =>
options.UseMySql(builder.Configuration["ConnectionStringsAkilliFiyat2:mysql_connection"], new MySqlServerVersion(new Version(8, 0, 30)))
);

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddIdentity<MyPortfolyoWebSite.Models.AppUser, MyPortfolyoWebSite.Models.AppRole>().AddEntityFrameworkStores<MyPortfolyoWebSite.Models.IdentityContext>().AddDefaultTokenProviders();
builder.Services.AddApplicationInsightsTelemetry();


builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.User.RequireUniqueEmail = true;
    // options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz";

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
});

builder.Services.AddHttpClient();

// ApiService ekleyin
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<MigrosIndirimUrunServices>();
builder.Services.AddScoped<CarfoursaIndirimUrunServices>();
builder.Services.AddScoped<BimIndirimUrunServices>();
builder.Services.AddScoped<A101IndirimUrunServices>();
builder.Services.AddScoped<KelimeKontrol>();
builder.Services.AddScoped<SokUrunServices>();
builder.Services.AddScoped<MyLogger>();
builder.Services.AddHostedService<NightlyTaskService>();
builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllerRoute(
    name: "post-details",
    pattern: "posts/details/{url}",
    defaults: new { controller = "Post", action = "Details" }
    );

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );

app.UseStaticFiles("/Areas/AkilliFiyatWeb");
app.UsePathBase("/Areas//AkilliFiyatWeb");

//app.MapControllerRoute(
//            name: "akilli-fiyat",
//            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
//
//        

app.MapAreaControllerRoute(
            name: "akilli-fiyat",
            areaName: "AkilliFiyatWeb",
            pattern: "akilli-fiyat/{controller=Home}/{action=Index}/{id?}"
        );
MyPortfolyoWebSite.Models.IdentitySeedData.IdentityTestUser(app);
AkilliFiyatWeb.Models.IdentitySeedData.IdentityTestUser(app);

app.Run();
