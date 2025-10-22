using CMCS.Data;
using CMCS.Models;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Services;

public class ClaimService
{
    private readonly AppDbContext _db;
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf", ".docx", ".xlsx", ".png", ".jpg", ".jpeg" };
    private const long MaxBytes = 5 * 1024 * 1024;

    public ClaimService(AppDbContext db) => _db = db;

    public async Task<Claim> CreateOrGetDraftAsync(int lecturerId, int year, int month)
    {
        var claim = await _db.Claims
            .Include(c => c.Items)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.LecturerId == lecturerId && c.Year == year && c.Month == month);

        if (claim is null)
        {
            claim = new Claim { LecturerId = lecturerId, Year = year, Month = month, HourlyRate = 0m };
            _db.Claims.Add(claim);
            await _db.SaveChangesAsync();
        }
        return claim;
    }

    public async Task AddLineItemAsync(int claimId, DateTime date, string module, decimal hours, string? notes)
    {
        if (hours < 0 || hours > 24) throw new ArgumentOutOfRangeException(nameof(hours));
        _db.ClaimLineItems.Add(new ClaimLineItem { ClaimId = claimId, Date = date, Module = module, Hours = hours, Notes = notes });
        await RecalculateAsync(claimId);
    }

    public async Task RecalculateAsync(int claimId)
    {
        var claim = await _db.Claims.Include(c => c.Items).FirstAsync(c => c.ClaimId == claimId);
        claim.TotalHours = claim.Items.Sum(i => i.Hours);
        claim.TotalAmount = claim.TotalHours * claim.HourlyRate;
        await _db.SaveChangesAsync();
    }

    public async Task SubmitAsync(int claimId)
    {
        var claim = await _db.Claims.Include(c => c.Items).FirstAsync(c => c.ClaimId == claimId);
        if (claim.Items.Count == 0) throw new InvalidOperationException("Add at least one line item.");
        claim.Status = ClaimStatus.Submitted;
        await _db.SaveChangesAsync();
    }

    public async Task<string> SaveDocumentAsync(int claimId, IFormFile file, IWebHostEnvironment env)
    {
        var ext = Path.GetExtension(file.FileName);
        if (!Allowed.Contains(ext)) throw new InvalidOperationException("Unsupported file type.");
        if (file.Length <= 0 || file.Length > MaxBytes) throw new InvalidOperationException("File too large (max 5 MB).");

        var dir = Path.Combine(env.WebRootPath, "uploads", claimId.ToString());
        Directory.CreateDirectory(dir);
        var safeName = string.Concat(Path.GetFileNameWithoutExtension(file.FileName).Take(60)) + ext;
        var path = Path.Combine(dir, safeName);
        using (var fs = File.Create(path)) { await file.CopyToAsync(fs); }

        var rel = $"/uploads/{claimId}/{safeName}";
        _db.SupportingDocuments.Add(new SupportingDocument
        {
            ClaimId = claimId,
            FileName = safeName,
            FileType = ext.Trim('.'),
            Url = rel
        });
        await _db.SaveChangesAsync();
        return rel;
    }

    public Task<List<Claim>> MyClaimsAsync(int lecturerId) =>
        _db.Claims.Where(c => c.LecturerId == lecturerId)
                  .OrderByDescending(c => c.CreatedAt).ToListAsync();
}
