namespace BatchRequest.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        [Route("{id}")]
        public string Get(int id) => id.ToString();
    }
}
