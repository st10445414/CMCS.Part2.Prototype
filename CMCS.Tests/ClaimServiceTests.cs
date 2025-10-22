using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMCS.Services;
using CMCS.Models;
using Xunit;

public class ClaimServiceTests : IDisposable
{
    private readonly TestFixture _fx = new();
    public void Dispose() => _fx.Dispose();

    [Fact]
    public async Task Submit_Fails_WithoutItems()
    {
        var svc = new ClaimService(_fx.Db);
        var claim = await svc.CreateOrGetDraftAsync(1, 2025, 9);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => svc.SubmitAsync(claim.ClaimId));
        Assert.Contains("least one line item", ex.Message);
    }

    [Fact]
    public async Task AddLineItems_Recalculates_Totals()
    {
        var svc = new ClaimService(_fx.Db);
        var claim = await svc.CreateOrGetDraftAsync(1, 2025, 9);
        _fx.Db.Claims.Find(claim.ClaimId)!.HourlyRate = 400m;

        await svc.AddLineItemAsync(claim.ClaimId, DateTime.Today, "ICT1", 2m, null);
        await svc.AddLineItemAsync(claim.ClaimId, DateTime.Today, "ICT2", 3.5m, null);

        var c = _fx.Db.Claims.Find(claim.ClaimId)!;
        Assert.Equal(5.5m, c.TotalHours);
        Assert.Equal(2200m, c.TotalAmount);
    }
}
