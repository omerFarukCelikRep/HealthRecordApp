using AutoMapper;
using Configuration.Messages;
using DataAccess.Abstract.IConfiguration;
using Entity.Concrete;
using Entity.Dtos.Generic;
using Entity.Dtos.Incoming.Profile;
using Entity.Dtos.Outgoing.Profile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProfileController : BaseController
{
    public ProfileController(
                             IUnitOfWork unitOfWork,
                             UserManager<IdentityUser> userManager,
                             IMapper mapper) : base(unitOfWork, userManager, mapper)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);
        var result = new Result<ProfileDto>();

        if (loggedInUser == null)
        {
            result.Error = PopulateError(400, ErrorMessages.Profile.UserNotFound, ErrorMessages.Generic.TypeBadRequest);

            return BadRequest(result);
        }

        var profile = await _unitOfWork.Users.GetByIdentityId(new Guid(loggedInUser.Id));

        if (profile == null)
        {
            result.Error = PopulateError(400, ErrorMessages.Profile.UserNotFound, ErrorMessages.Generic.TypeBadRequest);

            return BadRequest(result);
        }

        var mappedProfile = _mapper.Map<ProfileDto>(profile);

        result.Content = mappedProfile;

        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto profile)
    {
        var result = new Result<ProfileDto>();

        if (!ModelState.IsValid)
        {
            result.Error = PopulateError(400, ErrorMessages.Generic.InvalidPayload, ErrorMessages.Generic.TypeBadRequest);

            return BadRequest(result);
        }

        var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);

        if (loggedInUser == null)
        {
            result.Error = PopulateError(400, ErrorMessages.Profile.UserNotFound, ErrorMessages.Generic.TypeBadRequest);

            return BadRequest(result);
        }

        var userProfile = await _unitOfWork.Users.GetByIdentityId(new Guid(loggedInUser.Id));

        if (userProfile == null)
        {
            result.Error = PopulateError(400, ErrorMessages.Profile.UserNotFound, ErrorMessages.Generic.TypeBadRequest);

            return BadRequest(result);
        }

        userProfile.Address = profile.Address;
        userProfile.Sex = profile.Sex;
        userProfile.MobileNumber = profile.MobileNumber;
        userProfile.Country = profile.Country;

        var isUpdated = await _unitOfWork.Users.UpdateUserProfile(userProfile);

        if (isUpdated)
        {
            await _unitOfWork.CompleteAsync();

            var mappedProfile = _mapper.Map<ProfileDto>(userProfile);

            result.Content = mappedProfile;
            return Ok(result);
        }

        result.Error = PopulateError(500, ErrorMessages.Generic.SomethingWentWrong, ErrorMessages.Generic.UnableToProcess);

        return BadRequest(result);
    }
}