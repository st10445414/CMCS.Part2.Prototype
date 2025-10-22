using CMCS.Models;

namespace CMCS.Data;

public static class DemoSeed
{
    public static void Run(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var lec = new Lecturer { FullName = "Alex Dlamini", Email = "alex@example.edu", StaffNo = "IC1001" };
        var pc = new User { FullName = "Prog. Coordinator", Email = "pc@example.edu", Role = UserRole.ProgrammeCoordinator };
        var am = new User { FullName = "Academic Manager", Email = "am@example.edu", Role = UserRole.AcademicManager };

        db.Lecturers.Add(lec);
        db.Users.AddRange(pc, am);
        db.SaveChanges();

        var claim = new Claim { LecturerId = lec.LecturerId, Year = DateTime.Today.Year, Month = DateTime.Today.Month, HourlyRate = 400m };
        claim.Items.Add(new ClaimLineItem { Date = DateTime.Today.AddDays(-2), Module = "ICT1511", Hours = 2, Notes = "Tutoring" });
        claim.Items.Add(new ClaimLineItem { Date = DateTime.Today.AddDays(-1), Module = "ICT2612", Hours = 3.5m, Notes = "Marking" });
        claim.TotalHours = claim.Items.Sum(i => i.Hours);
        claim.TotalAmount = claim.TotalHours * claim.HourlyRate;
        db.Claims.Add(claim);
        db.SaveChanges();
    }
}
