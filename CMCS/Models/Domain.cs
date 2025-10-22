using System.ComponentModel.DataAnnotations;

namespace CMCS.Models;

public enum UserRole { Lecturer, ProgrammeCoordinator, AcademicManager }
public enum ClaimStatus { Draft, Submitted, UnderReview, Approved, Rejected, Returned, Settled }
public enum ApprovalStage { ProgrammeCoordinator, AcademicManager }
public enum Decision { Pending, Approved, Rejected, Returned }

public class Lecturer
{
    public int LecturerId { get; set; }
    [MaxLength(32)] public string StaffNo { get; set; } = "";
    [MaxLength(128)] public string FullName { get; set; } = "";
    [MaxLength(128)] public string Email { get; set; } = "";
    public List<Claim> Claims { get; set; } = new();
}

public class User
{
    public int UserId { get; set; }
    [MaxLength(128)] public string FullName { get; set; } = "";
    [MaxLength(128)] public string Email { get; set; } = "";
    public UserRole Role { get; set; }
}

public class Claim
{
    public int ClaimId { get; set; }
    public int LecturerId { get; set; }
    public Lecturer? Lecturer { get; set; }

    [Range(1, 12)] public int Month { get; set; }
    [Range(2000, 2100)] public int Year { get; set; }
    [Range(0, 10000)] public decimal HourlyRate { get; set; }

    public decimal TotalHours { get; set; }
    public decimal TotalAmount { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ClaimLineItem> Items { get; set; } = new();
    public List<SupportingDocument> Documents { get; set; } = new();
    public List<Approval> Approvals { get; set; } = new();

    public string Period() => $"{Year:D4}-{Month:D2}";
}

public class ClaimLineItem
{
    public int ClaimLineItemId { get; set; }
    public int ClaimId { get; set; }
    public Claim? Claim { get; set; }

    public DateTime Date { get; set; }
    [MaxLength(64)] public string Module { get; set; } = "";
    [Range(0, 24)] public decimal Hours { get; set; }
    public string? Notes { get; set; }
}

public class SupportingDocument
{
    public int SupportingDocumentId { get; set; }
    public int ClaimId { get; set; }
    public Claim? Claim { get; set; }

    [MaxLength(256)] public string FileName { get; set; } = "";
    [MaxLength(32)] public string FileType { get; set; } = "";
    [MaxLength(512)] public string Url { get; set; } = "";
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public class Approval
{
    public int ApprovalId { get; set; }
    public int ClaimId { get; set; }
    public Claim? Claim { get; set; }

    public int ApprovedByUserId { get; set; }
    public ApprovalStage Stage { get; set; }
    public Decision Decision { get; set; } = Decision.Pending;
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
