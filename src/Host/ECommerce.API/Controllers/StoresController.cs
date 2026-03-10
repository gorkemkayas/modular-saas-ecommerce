using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly ISender _sender;
        public StoresController(ISender sender)
        {
            _sender = sender;
        }

    }
}
