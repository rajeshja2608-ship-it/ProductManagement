using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProductManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        public IActionResult Get()
        {
            return Ok(new[]
            {
                new {Id=1,Name="Teacher 1", Subject="Maths"},
                new {Id=2,Name="Teacher 2", Subject="Science"},
                 new {Id=3,Name="Teacher 3", Subject="History"}
            });
        }
    }
}
