using HouseOfChess.Platform.Infrastructure.Contracts.Account;
using HouseOfChess.Platform.Infrastructure.Services.Account;
using HouseOfChess.Platform.WebAPI.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseOfChess.Platform.WebAPI.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public sealed class AccountController(IAccountService accounts) : ControllerBase
{
    [HttpPost("onboarding")]
    public async Task<ActionResult<OnboardingResponse>> Onboarding(
        [FromBody] OnboardingRequest request,
        CancellationToken ct)
    {
        var sub = User.GetAuth0Sub();
        if (string.IsNullOrWhiteSpace(sub))
            return Unauthorized();

        var result = await accounts.OnboardAsync(sub, request.Username, ct);

        return result.Outcome switch
        {
            OnboardingOutcome.Created          => Ok(new OnboardingResponse(result.UserId, result.Username, IsNew: true)),
            OnboardingOutcome.AlreadyOnboarded => Ok(new OnboardingResponse(result.UserId, result.Username, IsNew: false)),
            OnboardingOutcome.InvalidUsername  => BadRequest(new ProblemDetails { Title = result.Error, Status = StatusCodes.Status400BadRequest }),
            OnboardingOutcome.UsernameTaken    => Conflict(new ProblemDetails { Title = result.Error, Status = StatusCodes.Status409Conflict }),
            _                                  => Problem()
        };
    }
}
