using DataAccess.Abstract.IConfiguration;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class BaseController : ControllerBase
    {
        protected readonly IUnitOfWork _unitOfWork;

        public BaseController(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
    }
}
