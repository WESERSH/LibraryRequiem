using LibraryRequiem.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ���������� �������� � ���������.
builder.Services.AddControllersWithViews();

// ���������� ��������� ���� ������ � ���������.
builder.Services.AddDbContext<CollectionContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("CollectionContext")), ServiceLifetime.Transient);
builder.Services.AddScoped<IDbContextFactory<CollectionContext>, DbContextFactory<CollectionContext>>();
// ��������� �������������� ��� ������������� ����.
builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = new PathString("/Accounts/Login");
        options.AccessDeniedPath = new PathString("/Accounts/Login");
        options.LogoutPath = new PathString("/Accounts/Logout");
    });

// ��������� ���������� �����.
builder.Services.Configure<FormOptions>(opt => 
{ 
    opt.ValueCountLimit = int.MaxValue;
    opt.ValueLengthLimit = int.MaxValue;
    opt.MultipartBodyLengthLimit = int.MaxValue;
    opt.MemoryBufferThreshold = int.MaxValue;
});

var app = builder.Build();

// ��������� ��������� ��������� HTTP-��������.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // �������� HSTS �� ��������� ���������� 30 ����. ��������, ��� ��������� �������� ��� ��� ��������-���������, ��. https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

// ��������� ��������� ������������ �� ���������.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();