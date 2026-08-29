using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PRIV.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirst("sub")!.Value);
}
