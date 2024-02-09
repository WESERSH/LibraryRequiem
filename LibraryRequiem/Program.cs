/*using LibraryRequiem.Data;*/
using LibraryRequiem.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CollectionContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("CollectionContext")));
builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = new PathString("/Accounts/Login");
        options.AccessDeniedPath = new PathString("/Accounts/Login");
    });
builder.Services.Configure<FormOptions>(opt => 
{ 
    opt.ValueCountLimit = int.MaxValue;
    opt.ValueLengthLimit = int.MaxValue;
    opt.MultipartBodyLengthLimit = int.MaxValue;
    opt.MemoryBufferThreshold = int.MaxValue;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseAuthentication();
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();