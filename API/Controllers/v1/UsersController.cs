using DataAccess.Abstract.IConfiguration;
using Entity.Concrete;
using Entity.Dtos.Incoming;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers.v1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : BaseController
    {
        public UsersController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        //GET
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await _unitOfWork.Users.GetAll();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _unitOfWork.Users.GetById(id);

            return Ok(user);
        }

        //POST
        [HttpPost]
        public async Task<IActionResult> AddAsync(UserDto user)
        {
            var _user = new AppUser
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Country = user.Country,
                DateOfBirth = user.DateOfBirth,
                Phone = user.Phone
            };

            await _unitOfWork.Users.Add(_user);

            await _unitOfWork.CompleteAsync();

            return CreatedAtRoute(nameof(GetById), new { id = _user.Id }, user);
        }
    }
}
