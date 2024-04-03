using ABC.Common.DTO;
using ABC.Core.Contract;
using FluentValidation.Results;
using Macrin.Common;
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
    [RoutePrefix("api/v1/Items")]
    [ApiExceptionFilter]
    public class ItemsController : ApiController
    {
        readonly IItemsBL _bl;
        public ItemsController(IItemsBL itemsBL)
        {
            _bl = itemsBL;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Item>))]
        public async Task<IHttpActionResult> List([FromUri]Item filter, [FromUri]PageConfig pageConfig)
        {
            try
            {
                if (pageConfig == null)
                    pageConfig = new PageConfig();
                if (filter == null)
                    filter = new Item();
                return Ok(await _bl.List(filter, pageConfig));
            }
            catch (Exception e)
            {
                return Ok(e);
            }
        }

        [HttpGet]
        [Route("{Id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Item))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(int Id)
        {
            if (Id <= 0)
                return BadRequest(string.Format(PaginationErrorMessage.InvalidId, Id.ToString()));
            Item item = await _bl.GetByKeyAsync(Id);
            if (item.Id == Id) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(Item))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]Item model)
        {
            try
            {
                return await Save(model);
            }
            catch (Exception error)
            {
                return Ok(error);
            }
        }

        [HttpPut]
        [Route("{Id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Item))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(int Id, [FromBody]Item model)
        {
            if (model.Id != Id) return BadRequest("Resource Id's do not match.");
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Item))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(int Id, [FromBody]Item model)
        {
            if (model.Id != Id) return BadRequest("Resource Id's do not match.");
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        #region Helper Functions
        private async Task<IHttpActionResult> Save(Item model)
        {
            model = await _bl.SaveAsync(model);
            if (model.Validation.IsValid)
                return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        private void CreateModelState(ValidationResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }

        #endregion

    }
}
