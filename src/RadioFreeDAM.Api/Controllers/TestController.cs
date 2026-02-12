using Microsoft.AspNetCore.Mvc;
using RadioFreeDAM.Api.Data;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly AppDbContext _context;

    public TestController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Test()
    {
        return Ok(_context.RadioStations.Take(5).ToList());
    }
}
