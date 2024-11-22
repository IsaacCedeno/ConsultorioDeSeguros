using ConsultorioDeSeguros.Models;
using ConsultorioDeSeguros.Persistences.Interfaces;
using ConsultorioDeSeguros.Persistences.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("SegurosConnection");

builder.Services.AddScoped<IGenericRepository<Asegurado>>(provider => new AseguradoRepository(connectionString!));
builder.Services.AddScoped<IAseguradoRepository>(provider => new AseguradoRepository(connectionString!));
builder.Services.AddScoped<IGenericRepository<Seguro>>(provider => new SeguroRepository(connectionString!));



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
    name: "default",
    pattern: "{controller=Asegurado}/{action=Index}/{id?}");

app.Run();
