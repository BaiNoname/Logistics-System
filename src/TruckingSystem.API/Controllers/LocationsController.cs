using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Locations;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/locations")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _service;
    private readonly IPlaceSearchProvider _placeSearch;

    public LocationsController(ILocationService service, IPlaceSearchProvider placeSearch)
    {
        _service = service;
        _placeSearch = placeSearch;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<LocationDto>> Create(CreateLocationRequest request)
        => Ok(await _service.CreateAsync(request));

    /// <summary>Gõ tới đâu, gợi ý tới đó — dùng cho ô tìm kiếm địa điểm (VietMap Autocomplete).</summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<PlaceSuggestionDto>>> Search([FromQuery] string text)
        => Ok(await _placeSearch.SearchAsync(text));

    /// <summary>Lấy tọa độ thật của 1 gợi ý đã chọn (VietMap Place API).</summary>
    [HttpGet("place/{refId}")]
    public async Task<ActionResult<PlaceDetailDto>> GetPlaceDetail(string refId)
    {
        var detail = await _placeSearch.GetDetailAsync(refId);
        return detail is not null ? Ok(detail) : NotFound();
    }
}