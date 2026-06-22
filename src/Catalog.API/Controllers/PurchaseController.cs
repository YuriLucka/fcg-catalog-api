using System.Security.Claims;
using Catalog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchase;
    public PurchaseController(IPurchaseService purchase) => _purchase = purchase;

    public record PlaceOrderRequest(Guid GameId);

    [HttpPost]
    public async Task<IActionResult> Place([FromBody] PlaceOrderRequest request, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new UnauthorizedAccessException();
        var userId = Guid.Parse(sub);
        await _purchase.PlaceOrderAsync(userId, request.GameId, ct);
        return Accepted(new { message = "Compra em processamento." });
    }
}
