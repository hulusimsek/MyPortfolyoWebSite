//using AkilliFiyatWeb.Data;
//using AkilliFiyatWeb.Models;
//using AkilliFiyatWeb.Services;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddScoped<IEmailSender, SmtpEmailSender>(i =>
//    new SmtpEmailSender(
//        builder.Configuration["EmailSender:Host"],
//        builder.Configuration.GetValue<int>("EmailSender:Port"),
//        builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),
//        builder.Configuration["EmailSender:Username"],
//        builder.Configuration["EmailSender:Password"])
//);
//// Add services to the container.
//builder.Services.AddControllersWithViews();

//builder.Services.AddDbContext<DataContext>(options =>
//{
//    var config = builder.Configuration;
//    var connectionString = config.GetConnectionString("mysql_connection");
//    //options.UseSqlite(connectionString);

//    var version = new MySqlServerVersion(new Version(8, 0, 30));
//    options.UseMySql(connectionString, version);
//});

//builder.Services.AddDbContext<IdentityContext>(options =>
//options.UseMySql(builder.Configuration["ConnectionStrings2:mysql_connection"], new MySqlServerVersion(new Version(8, 0, 30)))
//);

//builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();

//builder.Services.Configure<IdentityOptions>(options =>
//{
//    options.Password.RequiredLength = 6;
//    options.Password.RequireLowercase = false;
//    options.Password.RequireUppercase = false;

//    options.User.RequireUniqueEmail = true;
//    // options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz";

//    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
//    options.Lockout.MaxFailedAccessAttempts = 5;
//});

//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = "/Account/Login";
//    options.AccessDeniedPath = "/Account/AccessDenied";
//    options.SlidingExpiration = true;
//    options.ExpireTimeSpan = TimeSpan.FromDays(30);
//});

//builder.Services.AddHttpClient();

//// ApiService ekleyin
//builder.Services.AddHostedService<NightlyTaskService>();
//builder.Services.AddScoped<ApiService>();
//builder.Services.AddScoped<MigrosIndirimUrunServices>();
//builder.Services.AddScoped<CarfoursaIndirimUrunServices>();
//builder.Services.AddScoped<BimIndirimUrunServices>();
//builder.Services.AddScoped<A101IndirimUrunServices>();
//builder.Services.AddScoped<KelimeKontrol>();
//builder.Services.AddScoped<SokUrunServices>();

//builder.Services.AddScoped<MyLogger>();

//builder.Services.AddControllersWithViews();
//builder.WebHost.UseStaticWebAssets();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllerRoute(name: "default",
//                       pattern: "{controller=Home}/{action=Index}/{id?}");

//IdentitySeedData.IdentityTestUser(app);

//app.Run();