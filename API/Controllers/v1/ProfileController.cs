using DataAccess.Abstract.IConfiguration;
using Entity.Dtos.Incoming.Profile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers.v1;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProfileController : BaseController
{
    public ProfileController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager) : base(unitOfWork, userManager)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);

        if (loggedInUser == null)
        {
            return BadRequest("User Not Found");
        }

        var profile = await _unitOfWork.Users.GetByIdentityId(new Guid(loggedInUser.Id));

        if (profile == null)
        {
            return BadRequest("User Not Found");
        }

        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto profile)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid Payload");
        }

        var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);

        if (loggedInUser == null)
        {
            return BadRequest("User Not Found");
        }

        var userProfile = await _unitOfWork.Users.GetByIdentityId(new Guid(loggedInUser.Id));

        if (userProfile == null)
        {
            return BadRequest("User Not Found");
        }

        userProfile.Address = profile.Address;
        userProfile.Sex = profile.Sex;
        userProfile.MobileNumber = profile.MobileNumber;
        userProfile.Country = profile.Country;

        var isUpdated = await _unitOfWork.Users.UpdateUserProfile(userProfile);

        if (isUpdated)
        {
            await _unitOfWork.CompleteAsync();
            return Ok(profile);
        }

        return BadRequest("Something went wrong, please try again later");
    }
}