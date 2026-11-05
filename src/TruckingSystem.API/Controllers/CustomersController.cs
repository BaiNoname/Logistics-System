using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TruckingSystem.Application.Customers;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Admin,Dispatcher,Accountant")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;
    private readonly ICustomerBusinessCardReader _businessCardReader;

    public CustomersController(ICustomerService service, ICustomerBusinessCardReader businessCardReader)
    {
        _service = service;
        _businessCardReader = businessCardReader;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerListItemDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<CustomerActionResult>> Create(CreateCustomerRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<CustomerActionResult>> Update(int id, UpdateCustomerRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("contacts")]
    public async Task<ActionResult<List<CustomerContactListItemDto>>> GetContacts([FromQuery] int? customerId)
        => Ok(await _service.GetContactsAsync(customerId));

    [HttpPost("contacts")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<CustomerActionResult>> CreateContact(SaveCustomerContactRequest request)
    {
        var result = await _service.CreateContactAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("contacts/{id:int}")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<CustomerActionResult>> UpdateContact(int id, SaveCustomerContactRequest request)
    {
        var result = await _service.UpdateContactAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("contacts/{id:int}")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public async Task<ActionResult<CustomerActionResult>> DeleteContact(int id)
    {
        var result = await _service.DeleteContactAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("read-business-card")]
    [Authorize(Roles = "Admin,Dispatcher")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024 + 64 * 1024)]
    public async Task<ActionResult<CustomerBusinessCardInfo>> ReadBusinessCard(IFormFile image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0) return BadRequest("Vui lòng chọn ảnh danh thiếp.");
        if (image.Length > 10 * 1024 * 1024) return BadRequest("Ảnh danh thiếp tối đa 10 MB.");
        if (image.ContentType is not ("image/jpeg" or "image/png")) return BadRequest("Chỉ hỗ trợ ảnh JPG hoặc PNG.");

        await using var stream = new MemoryStream();
        await image.CopyToAsync(stream, cancellationToken);
        try
        {
            var result = await _businessCardReader.ReadAsync(stream.ToArray(), image.ContentType, cancellationToken);
            if (result.IsNotBusinessCard)
                return UnprocessableEntity(new { message = result.ErrorMessage, result.ErrorMessage, result.IsNotBusinessCard });
            return result.ErrorMessage is not null && !result.HasExtractedFields()
                ? StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage, result.ErrorMessage, result.RawResponse })
                : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = "Gemini phản hồi quá lâu. Vui lòng thử lại sau." });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (JsonException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "AI trả về dữ liệu không đúng định dạng." });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }
}
