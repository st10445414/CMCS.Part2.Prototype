using CMCS.Models;
using CMCS.Services;
using Microsoft.AspNetCore.Mvc;

namespace CMCS.Controllers;

public class ApprovalsController : Controller
{
    private readonly ApprovalService _approvals;

    private const int DemoPCUserId = 2;
    private const int DemoAMUserId = 3;

    public ApprovalsController(ApprovalService approvals) => _approvals = approvals;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var list = await _approvals.PendingAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeDecision(int claimId, string stage, string action, string? comment)
    {
        var stg = Enum.Parse<ApprovalStage>(stage);
        var userId = stg == ApprovalStage.ProgrammeCoordinator ? DemoPCUserId : DemoAMUserId;

        var dec = action.ToLowerInvariant() switch
        {
            "approve" => CMCS.Models.Decision.Approved,
            "reject" => CMCS.Models.Decision.Rejected,
            "return" => CMCS.Models.Decision.Returned,
            _ => CMCS.Models.Decision.Pending
        };

        await _approvals.DecideAsync(claimId, userId, stg, dec, comment);
        TempData["Toast"] = $"Decision recorded: {dec}";
        return RedirectToAction(nameof(Index));
    }
}

