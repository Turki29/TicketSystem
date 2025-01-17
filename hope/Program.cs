using Microsoft.EntityFrameworkCore;
using TicketSystem.Data;
using Microsoft.AspNetCore.Identity;
using Utility;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();// عشان إنشاء الحساب والتسجيل
var connString =Environment.GetEnvironmentVariable("TICKET_SYS_DB_KEY");
if(string.IsNullOrEmpty(connString))
{
    throw new InvalidOperationException("Connection string 'TICKET_SYS_DB_KEY' not found.");

}

builder.Services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(connString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddScoped<IEmailSender, EmailSender>();
/*
 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
أضفناه لأننا غيرنا خدمة تسجيل المستخدمين الافتراضية 
 
الافتراضية : AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
الحالية   : AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
وأضفنا : AddDefaultTokenProviders();
 */

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenIed";
});
/*
 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
تعديل لمسار التسجيل الافتراضي
 */


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

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages(); // عشان إنشاء الحساب والتسجيل

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Home}/{controller=Home}/{action=Index}/{id?}");

app.Run();
