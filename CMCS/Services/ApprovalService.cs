using CMCS.Data;
using CMCS.Models;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Services;

public class ApprovalService
{
    private readonly AppDbContext _db;
    public ApprovalService(AppDbContext db) => _db = db;

    public Task<List<Claim>> PendingAsync() =>
        _db.Claims.Include(c => c.Lecturer)
                  .Where(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview || c.Status == ClaimStatus.Returned)
                  .OrderBy(c => c.CreatedAt).ToListAsync();

    public async Task DecideAsync(int claimId, int byUserId, ApprovalStage stage, Decision decision, string? comment)
    {
        var claim = await _db.Claims.FirstAsync(c => c.ClaimId == claimId);

        _db.Approvals.Add(new Approval
        {
            ClaimId = claimId,
            ApprovedByUserId = byUserId,
            Stage = stage,
            Decision = decision,
            Comment = comment
        });

        claim.Status = decision switch
        {
            Decision.Approved when stage == ApprovalStage.ProgrammeCoordinator => ClaimStatus.UnderReview,
            Decision.Approved when stage == ApprovalStage.AcademicManager => ClaimStatus.Approved,
            Decision.Rejected => ClaimStatus.Rejected,
            Decision.Returned => ClaimStatus.Returned,
            _ => ClaimStatus.UnderReview
        };

        await _db.SaveChangesAsync();
    }
}
