using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Auth;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize(Roles = "Admin")]
public class AccountsController : ControllerBase
{
    private readonly IAccountManagementService _service;

    public AccountsController(IAccountManagementService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("roles")]
    public async Task<ActionResult<List<string>>> GetRoles()
        => Ok(await _service.GetRolesAsync());

    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create(CreateAccountRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result.Success ? Ok(result.Account) : BadRequest(result.Message);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateAccountRequest request)
    {
        var result = await _service.UpdateAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier), request);
        return result.NotFound ? NotFound() : result.Success ? NoContent() : BadRequest(result.Message);
    }

    [HttpPut("{id}/password")]
    public async Task<IActionResult> ResetPassword(string id, ResetPasswordRequest request)
    {
        var result = await _service.ResetPasswordAsync(id, request);
        return result.NotFound ? NotFound() : result.Success ? NoContent() : BadRequest(result.Message);
    }
}
