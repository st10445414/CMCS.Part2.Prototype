using System;
using CMCS.Data;
using CMCS.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// why: lightweight persistence for Part 2
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=cmcs.db"));

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ClaimService>();
builder.Services.AddScoped<ApprovalService>();

var app = builder.Build();

// seed + ensure db
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DemoSeed.Run(db); // why: demo lecturer & pending claims
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
