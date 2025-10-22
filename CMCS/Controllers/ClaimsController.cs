using CMCS.Data;
using CMCS.Models;
using CMCS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Controllers;

public class ClaimsController : Controller
{
    private readonly AppDbContext _db;
    private readonly ClaimService _claims;
    private readonly IWebHostEnvironment _env;

    // why: assume single demo lecturer (id=1) for simplicity
    private const int DemoLecturerId = 1;

    public ClaimsController(AppDbContext db, ClaimService claims, IWebHostEnvironment env)
    { _db = db; _claims = claims; _env = env; }

    [HttpGet]
    public async Task<IActionResult> Submit(int? month, int? year)
    {
        var m = month ?? DateTime.Today.Month;
        var y = year ?? DateTime.Today.Year;
        var claim = await _claims.CreateOrGetDraftAsync(DemoLecturerId, y, m);
        await _db.Entry(claim).Collection(c => c.Items).LoadAsync();
        await _db.Entry(claim).Collection(c => c.Documents).LoadAsync();
        return View(claim);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(int claimId, DateTime date, string module, decimal hours, string? notes)
    {
        await _claims.AddLineItemAsync(claimId, date, module, hours, notes);
        return RedirectToAction(nameof(Submit), new { month = (await _db.Claims.FindAsync(claimId))!.Month, year = (await _db.Claims.FindAsync(claimId))!.Year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int claimId, IFormFile file)
    {
        if (file is null || file.Length == 0) { TempData["Toast"] = "Select a file."; return RedirectToAction(nameof(Submit)); }
        try
        {
            await _claims.SaveDocumentAsync(claimId, file, _env);
            TempData["Toast"] = "File uploaded.";
        }
        catch (Exception ex)
        {
            TempData["Toast"] = ex.Message;
        }
        return RedirectToAction(nameof(Submit), new { month = (await _db.Claims.FindAsync(claimId))!.Month, year = (await _db.Claims.FindAsync(claimId))!.Year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitClaim(int claimId)
    {
        try
        {
            await _claims.SubmitAsync(claimId);
            TempData["Toast"] = "Claim submitted.";
        }
        catch (Exception ex)
        {
            TempData["Toast"] = ex.Message;
        }
        return RedirectToAction(nameof(Status));
    }

    [HttpGet]
    public async Task<IActionResult> Status()
    {
        var claims = await _claims.MyClaimsAsync(DemoLecturerId);
        return View(claims);
    }
}

