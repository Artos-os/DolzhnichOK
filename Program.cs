using DolzhnikOk.Components;
using DolzhnikOk.Data;
using DolzhnikOk.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) && builder.Environment.IsDevelopment())
{
    connectionString = "Host=localhost;Port=5433;Database=dolzhnikok;Username=dolzhnikok;Password=dolzhnikok_dev_only";
}
else if (!string.IsNullOrWhiteSpace(connectionString) && builder.Environment.IsDevelopment())
{
    // The locally installed PostgreSQL service listens on port 5433.
    var localConnection = new Npgsql.NpgsqlConnectionStringBuilder(connectionString) { Port = 5433 };
    connectionString = localConnection.ConnectionString;
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Не задана строка подключения DefaultConnection.");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Регистрируем компоненты Blazor и включаем интерактивность на сервере.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

await BootstrapInitializer.InitializeAsync(app.Services, app.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapPost("/account/login", async (HttpContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AppDbContext db, Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery) =>
{
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException) { return Results.BadRequest("Форма устарела. Обновите страницу."); }
    var form = await context.Request.ReadFormAsync();
    var userName = form["UserName"].ToString().Trim();
    var password = form["Password"].ToString();
    var role = form["Role"].ToString();
    if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password) || role is not ("Student" or "Teacher"))
        return Results.Redirect("/?error=invalid");
    var user = await userManager.FindByNameAsync(userName);
    if (user is null || !user.IsActive || !await userManager.IsInRoleAsync(user, role))
        return Results.Redirect("/?error=invalid");
    var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: true);
    if (!result.Succeeded) return Results.Redirect("/?error=invalid");
    var destination = role == "Student"
        ? (await db.Students.AnyAsync(x => x.UserId == user.Id) ? "/student" : "/?error=profile")
        : (await db.Teachers.AnyAsync(x => x.UserId == user.Id) ? "/prepodavatel" : "/?error=profile");
    if (destination.StartsWith("/?")) await signInManager.SignOutAsync();
    return Results.LocalRedirect(destination);
}).DisableAntiforgery();
app.MapPost("/account/logout", async (HttpContext context, SignInManager<ApplicationUser> signInManager, Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery) =>
{
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException) { return Results.BadRequest("Форма устарела. Обновите страницу."); }
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
}).DisableAntiforgery();
app.MapPost("/account/register-student", async (HttpContext context, AppDbContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery) =>
{
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException) { return Results.BadRequest("Форма устарела. Обновите страницу."); }
    var form = await context.Request.ReadFormAsync();
    var userName = form["UserName"].ToString().Trim();
    var email = form["Email"].ToString().Trim();
    var fullName = form["FullName"].ToString().Trim();
    var password = form["Password"].ToString();
    if (!int.TryParse(form["GroupId"], out var groupId) || fullName.Length is < 2 or > 100)
        return Results.Redirect("/registratsiya?error=register");
    var group = await db.StudentGroups.FindAsync(groupId);
    if (group is null) return Results.Redirect("/registratsiya?error=register");
    await using var tx = await db.Database.BeginTransactionAsync();
    var user = new ApplicationUser { UserName = userName, Email = email };
    var created = await userManager.CreateAsync(user, password);
    if (!created.Succeeded) { await tx.RollbackAsync(); return Results.Redirect("/registratsiya?error=register"); }
    var role = await userManager.AddToRoleAsync(user, "Student");
    if (!role.Succeeded) { await tx.RollbackAsync(); return Results.Redirect("/registratsiya?error=register"); }
    db.Students.Add(new StudentProfile { UserId = user.Id, FullName = fullName, GroupId = groupId });
    await db.SaveChangesAsync();
    await tx.CommitAsync();
    await signInManager.SignInAsync(user, isPersistent: true);
    return Results.LocalRedirect("/student");
}).DisableAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
