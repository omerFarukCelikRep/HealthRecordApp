using AutoMapper;
using Configuration.Messages;
using DataAccess.Abstract.IConfiguration;
using Entity.Concrete;
using Entity.Dtos.Generic;
using Entity.Dtos.Incoming;
using Entity.Dtos.Outgoing.Profile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UsersController : BaseController
{
    public UsersController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IMapper mapper) : base(unitOfWork, userManager, mapper)
    {
    }

    //GET
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = new PagedResult<AppUser>();
        var users = await _unitOfWork.Users.GetAll();

        result.Content = users.ToList();
        result.ResultCount = users.Count();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = new Result<ProfileDto>();
        var user = await _unitOfWork.Users.GetById(id);

        if (user != null)
        {
            var mappedProfile = _mapper.Map<ProfileDto>(user);
            result.Content = mappedProfile;

            return Ok(result);
        }

        result.Error = PopulateError(404, ErrorMessages.Users.UserNotFound, ErrorMessages.Generic.ObjectNotFound);

        return BadRequest(result);
    }

    //POST
    [HttpPost("Add")]
    public async Task<IActionResult> AddAsync(UserDto user)
    {
        var _mappedUser = _mapper.Map<AppUser>(user);

        await _unitOfWork.Users.Add(_mappedUser);

        await _unitOfWork.CompleteAsync();

        var result = new Result<UserDto>();
        result.Content = user;

        return CreatedAtRoute(nameof(GetById), new { id = _mappedUser.Id }, user);
    }
}