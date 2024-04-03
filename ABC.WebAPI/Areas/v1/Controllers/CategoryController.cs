using ABC.Common.DTO;
using ABC.Core.Contract;
using Macrin.WebApi;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ABC.WebAPI.Areas.v1.Controllers
{
    [RoutePrefix("api/v1/Categories")]
    [ApiExceptionFilter]
    public class CategoryController : ApiController
    {
        readonly ICategoryBL _bl;

        public CategoryController(ICategoryBL categoryBL)
        {
            _bl = categoryBL;
        }

        [HttpGet]
        [Route("{Id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Category))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(int Id)
        {
            if (Id <= 0)
                return BadRequest(string.Format(PaginationErrorMessage.InvalidId, Id.ToString()));
            Category item = await _bl.GetByKeyAsync(Id);
            if (item.Id == Id) return Ok(item);
            return NotFound();
        }
    }
}

