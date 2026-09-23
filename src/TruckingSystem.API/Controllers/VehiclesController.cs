using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Vehicles;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _service;

    public VehiclesController(IVehicleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<VehicleListItemDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<VehicleListItemDto>> Create(CreateVehicleRequest request)
        => Ok(await _service.CreateAsync(request));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VehicleActionResult>> Update(int id, UpdateVehicleRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{vehicleId:int}/documents")]
    public async Task<ActionResult<List<VehicleDocumentDto>>> GetDocuments(int vehicleId)
        => Ok(await _service.GetDocumentsAsync(vehicleId));

    [HttpPost("{vehicleId:int}/documents")]
    public async Task<ActionResult<VehicleDocumentDto>> CreateDocument(int vehicleId, CreateVehicleDocumentRequest request)
    {
        try
        {
            return Ok(await _service.CreateDocumentAsync(vehicleId, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("documents/{documentId:int}")]
    public async Task<ActionResult<VehicleActionResult>> DeleteDocument(int documentId)
    {
        var result = await _service.DeleteDocumentAsync(documentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}