using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APlS.Errors;
using Talabat.Repository.Data;

namespace Talabat.APlS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuggyController : APIBaseController
    {
        private readonly StoreContext _dbContext;

        public BuggyController(StoreContext dbContext) {
            this._dbContext = dbContext;
        }



        [HttpGet("NotFound")]
        public ActionResult GetNotFoundRequest()
        {
            var Product = _dbContext.Products.Find(100);
            if (Product is null)
            {
                return NotFound(new ApiResponse(404));
            }
            return Ok(Product);
        }

        [HttpGet("ServerError")]
        public ActionResult GetServerError()
        {
            var Product = _dbContext.Products.Find(100);
            var ProductToReturn = Product.ToString();
            return Ok(ProductToReturn);
        }

        [HttpGet("BadRequest")]
        public ActionResult GetBadRequest()
        {
            return BadRequest();
        }

        [HttpGet("BadRequest/{id}")]
        public ActionResult GetBadRequest(int id)
        {
            return Ok();
        }
    }
}
