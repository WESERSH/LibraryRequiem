using LibraryRequiem.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов в контейнер.
builder.Services.AddControllersWithViews();

// Добавление контекста базы данных в контейнер.
builder.Services.AddDbContext<CollectionContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("CollectionContext")), ServiceLifetime.Transient);
builder.Services.AddScoped<IDbContextFactory<CollectionContext>, DbContextFactory<CollectionContext>>();
// Настройка аутентификации для использования куки.
builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = new PathString("/Accounts/Login");
        options.AccessDeniedPath = new PathString("/Accounts/Login");
        options.LogoutPath = new PathString("/Accounts/Logout");
    });

// Настройка параметров формы.
builder.Services.Configure<FormOptions>(opt => 
{ 
    opt.ValueCountLimit = int.MaxValue;
    opt.ValueLengthLimit = int.MaxValue;
    opt.MultipartBodyLengthLimit = int.MaxValue;
    opt.MemoryBufferThreshold = int.MaxValue;
});

var app = builder.Build();

// Настройка конвейера обработки HTTP-запросов.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Значение HSTS по умолчанию составляет 30 дней. Возможно, вам захочется изменить это для продакшн-сценариев, см. https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

// Настройка маршрутов контроллеров по умолчанию.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();