using Authentication.Configuration;
using Authentication.Models.Dtos.Incoming;
using Authentication.Models.Dtos.Outgoing;
using DataAccess.Abstract.IConfiguration;
using Entity.Concrete;
using Entity.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers.v1
{
    public class AccountsController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        public AccountsController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor) : base(unitOfWork)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto registrationRequestDto)
        {
            if (ModelState.IsValid)
            {
                //Check if email already exist
                var userExist = await _userManager.FindByEmailAsync(registrationRequestDto.Email);

                if (userExist != null)
                {
                    return BadRequest(new UserRegistrationResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Email already taken"
                        }
                    });
                }

                //Add user
                var newUser = new IdentityUser()
                {
                    Email = registrationRequestDto.Email,
                    UserName = registrationRequestDto.Email,
                    EmailConfirmed = true //TODO: Build functionality to send user to confirm email
                };

                var isCreated = await _userManager.CreateAsync(newUser, registrationRequestDto.Password);

                if (!isCreated.Succeeded) //when registration has failed
                {
                    return BadRequest(new UserRegistrationResponseDto()
                    {
                        Success = false,
                        Errors = isCreated.Errors.Select(a => a.Description).ToList()
                    });
                }

                var _user = new AppUser();
                _user.IdentityId = new Guid(newUser.Id);
                _user.FirstName = registrationRequestDto.FirstName;
                _user.LastName = registrationRequestDto.LastName;
                _user.Email = registrationRequestDto.Email;
                _user.DateOfBirth = DateTime.UtcNow; //TODO: Convert.ToDateTime(user.DateOfBirth)
                _user.Phone = "";
                _user.Country = "";

                await _unitOfWork.Users.Add(_user);
                await _unitOfWork.CompleteAsync();

                //create Jwt Token
                var token = GenerateJwtToken(newUser);

                //return to user
                return Ok(new UserRegistrationResponseDto()
                {
                    Success = true,
                    Token = token
                });
            }
            else
            {
                return BadRequest(new UserRegistrationResponseDto
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Invalid Payload"
                    }
                });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginRequestDto)
        {
            if (ModelState.IsValid)
            {
                //Check if email exist
                var userExist = await _userManager.FindByEmailAsync(loginRequestDto.Email);

                if (userExist == null)
                {
                    return BadRequest(new UserLoginResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Invalid authentication request"
                        }
                    });
                }

                //Check if the user has a valid password
                var isCorrect = await _userManager.CheckPasswordAsync(userExist, loginRequestDto.Password);

                if (isCorrect)
                {
                    var jwtToken = GenerateJwtToken(userExist);

                    return Ok(new UserLoginResponseDto()
                    {
                        Success = true,
                        Token = jwtToken
                    });
                }
                else
                {
                    return BadRequest(new UserLoginResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Invalid authentication request"
                        }
                    });
                }
            }
            else
            {
                return BadRequest(new UserRegistrationResponseDto
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Invalid Payload"
                    }
                });
            }
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            //The handler is going to be responsible for creating the token
            var jwtHandler = new JwtSecurityTokenHandler();

            //Get security key
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) //Used by the refreshed token

                }),
                Expires = DateTime.UtcNow.AddHours(3), //TODO: Update expiration time
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature //TODO: Review the algorithm
                )
            };

            //Generate the security token
            var token = jwtHandler.CreateToken(tokenDescriptor);

            //Convert the security obj token into a string
            var jwtToken = jwtHandler.WriteToken(token);

            return jwtToken;

        }
    }
}
