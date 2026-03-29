using MediatR;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Stores.Queries.GetPublishedStorefrontBySlug;
using Store.Application.DTOs;

namespace ECommerce.API.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorefrontController : ControllerBase
    {
        private readonly ISender _sender;

        public StorefrontController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("{slug}")]
        [ProducesResponseType(typeof(StorefrontDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPublishedStoreFrontBySlug([FromRoute]string slug, CancellationToken cancellationToken)
        {
            var res = await _sender.Send(new GetPublishedStoreFrontBySlugQuery(slug), cancellationToken);
            return res is not null ? Ok(res) : NotFound();
        }
    }
}
