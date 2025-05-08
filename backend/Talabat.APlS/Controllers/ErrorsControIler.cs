using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Talabat.APlS.Errors;

namespace Talabat.APlS.Controllers
{
    [Route("errors/{code}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsControIler : ControllerBase
    {
        
        public ActionResult Error(int code)
        {
            return NotFound(new ApiResponse(code));
        }
    }
}
