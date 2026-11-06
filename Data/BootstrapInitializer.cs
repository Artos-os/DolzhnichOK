using DolzhnikOk.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DolzhnikOk.Data;

public static class BootstrapInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in new[] { "Student", "Teacher" })
            if (!await roles.RoleExistsAsync(roleName))
                Ensure(await roles.CreateAsync(new IdentityRole<int>(roleName)), $"Не удалось создать роль {roleName}.");

        var departmentName = configuration["Bootstrap:Department"] ?? "Общая кафедра";
        var department = await db.Departments.SingleOrDefaultAsync(x => x.Name == departmentName);
        if (department is null)
        {
            department = new Department { Name = departmentName };
            db.Departments.Add(department);
            await db.SaveChangesAsync();
        }

        var groupCode = configuration["Bootstrap:Group"] ?? "Общая группа";
        if (!await db.StudentGroups.AnyAsync(x => x.DepartmentId == department.Id && x.Code == groupCode))
            db.StudentGroups.Add(new StudentGroup { DepartmentId = department.Id, Code = groupCode });
        var subjectName = configuration["Bootstrap:Subject"] ?? "Без указания предмета";
        if (!await db.Subjects.AnyAsync(x => x.DepartmentId == department.Id && x.Name == subjectName))
            db.Subjects.Add(new Subject { DepartmentId = department.Id, Name = subjectName });
        var roomNumber = configuration["Bootstrap:Classroom"] ?? "101";
        if (!await db.Classrooms.AnyAsync(x => x.DepartmentId == department.Id && x.Number == roomNumber))
            db.Classrooms.Add(new Classroom { DepartmentId = department.Id, Number = roomNumber });
        await db.SaveChangesAsync();

        var userName = configuration["Bootstrap:Teacher:UserName"];
        var password = configuration["Bootstrap:Teacher:Password"];
        var email = configuration["Bootstrap:Teacher:Email"];
        var fullName = configuration["Bootstrap:Teacher:FullName"];
        if (new[] { userName, password, email, fullName }.All(string.IsNullOrWhiteSpace)) return;
        if (new[] { userName, password, email, fullName }.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Для первого преподавателя задайте Bootstrap:Teacher:UserName, Password, Email и FullName.");

        var teacherUser = await users.FindByNameAsync(userName!);
        if (teacherUser is null)
        {
            teacherUser = new ApplicationUser { UserName = userName, Email = email, EmailConfirmed = true };
            Ensure(await users.CreateAsync(teacherUser, password!), "Не удалось создать первого преподавателя.");
        }
        if (!await users.IsInRoleAsync(teacherUser, "Teacher"))
            Ensure(await users.AddToRoleAsync(teacherUser, "Teacher"), "Не удалось назначить преподавателю роль.");
        if (!await db.Teachers.AnyAsync(x => x.UserId == teacherUser.Id))
            db.Teachers.Add(new TeacherProfile { UserId = teacherUser.Id, FullName = fullName!, DepartmentId = department.Id });
        await db.SaveChangesAsync();
    }

    private static void Ensure(IdentityResult result, string message)
    {
        if (!result.Succeeded) throw new InvalidOperationException($"{message} {string.Join("; ", result.Errors.Select(x => x.Description))}");
    }
}
