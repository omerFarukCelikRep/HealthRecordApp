using Authentication.Configuration;
using Authentication.Models.Dtos.Generic;
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
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly JwtConfig _jwtConfig;
        public AccountsController(IUnitOfWork unitOfWork,
                                    UserManager<IdentityUser> userManager,
                                    IOptionsMonitor<JwtConfig> optionsMonitor,
                                    TokenValidationParameters tokenValidationParameters) : base(unitOfWork, userManager)
        {
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParameters = tokenValidationParameters;
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
                var token = await GenerateJwtToken(newUser);

                //return to user
                return Ok(new UserRegistrationResponseDto()
                {
                    Success = true,
                    Token = token.JwtToken,
                    RefreshToken = token.RefreshToken
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
                    var jwtToken = await GenerateJwtToken(userExist);

                    return Ok(new UserLoginResponseDto()
                    {
                        Success = true,
                        Token = jwtToken.JwtToken,
                        RefreshToken = jwtToken.RefreshToken
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

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
        {
            if (ModelState.IsValid)
            {
                //Check if the token is valid
                var result = await VerifyToken(tokenRequestDto);

                if (result == null)
                {
                    return BadRequest(new UserRegistrationResponseDto
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token Validation Failed"
                        }
                    });
                }

                return Ok(result);
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

        private async Task<AuthResult> VerifyToken(TokenRequestDto tokenRequestDto)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                //We need to check the validity of the token
                var principal = tokenHandler.ValidateToken(tokenRequestDto.Token, _tokenValidationParameters, out var validatedToken);

                //We need to validate the results that has been generated for us
                //Validate if the string is an actual JWT Token not a random string
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    //Check if the JWT token created with the same algorithm as our JWt token
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (!result)
                    {
                        return null;
                    }
                }

                //We need to check the expiry date of token
                var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(a => a.Type == JwtRegisteredClaimNames.Exp).Value);

                //Convert to date to check
                var expDate = UnixTimeStampToDateTime(utcExpiryDate);

                //Checking if the jwt token has expired
                if (expDate > DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "JWT token has not expired"
                        }
                    };
                }

                //Check if the refresh token exist
                var refreshTokenExist = await _unitOfWork.RefreshTokens.GetByRefreshToken(tokenRequestDto.RefreshToken);

                if (refreshTokenExist == null)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Invalid Refresh Token"
                        }
                    };
                }

                //Check the expiry date of refresh token 
                if (refreshTokenExist.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh Token has expired, please login again"
                        }
                    };
                }

                //Check if the refreh token has been used or not
                if (refreshTokenExist.IsUsed)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh Token has been used, it cannot be reused"
                        }
                    };
                }

                //Check refresh token if it has been revoked
                if (refreshTokenExist.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh Token has been revoked, it cannot be used"
                        }
                    };
                }

                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if (refreshTokenExist.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh Token reference does not match the Jwt Token"
                        }
                    };
                }

                //Start processing and get a new token
                refreshTokenExist.IsUsed = true;

                var updateResult = await _unitOfWork.RefreshTokens.MarkRefreshTokenAsUsed(refreshTokenExist);

                if (updateResult)
                {
                    await _unitOfWork.CompleteAsync();

                    //Get the user to generate a new Jwt Token
                    var appUser = await _userManager.FindByIdAsync(refreshTokenExist.UserId);

                    if (appUser == null)
                    {
                        return new AuthResult()
                        {
                            Success = false,
                            Errors = new List<string>()
                            {
                                "Error processing request"
                            }
                        };
                    }

                    //Generate a Jwt Token
                    var tokens = await GenerateJwtToken(appUser);

                    return new AuthResult
                    {
                        Token = tokens.JwtToken,
                        Success = true,
                        RefreshToken = tokens.RefreshToken
                    };
                }

                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Error processing request"
                    }
                };
            }
            catch (Exception ex)
            {
                //TODO: Add Better Error Handling
                //TODO:Add a Logger
                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixDate)
        {
            //Set the time to 1, Jan 1970
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            //Add the number of seconds from 1 Jan 1970
            dateTime = dateTime.AddSeconds(unixDate).ToUniversalTime();

            return dateTime;
        }

        private async Task<TokenData> GenerateJwtToken(IdentityUser user)
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
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) //Used by the refreshed token

                }),
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTimeFrame), //TODO: Update expiration time
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature //TODO: Review the algorithm
                )
            };

            //Generate the security token
            var token = jwtHandler.CreateToken(tokenDescriptor);

            //Convert the security obj token into a string
            var jwtToken = jwtHandler.WriteToken(token);

            //Generate a refresh token
            var refreshToken = new RefreshToken
            {
                CreatedDate = DateTime.UtcNow,
                Token = $"{ RandomStringGenerator(25)}_{Guid.NewGuid()}",
                UserId = user.Id,
                IsRevoked = false,
                IsUsed = false,
                Status = Status.Added,
                JwtId = token.Id,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            await _unitOfWork.RefreshTokens.Add(refreshToken);
            await _unitOfWork.CompleteAsync();

            var tokenData = new TokenData
            {
                JwtToken = jwtToken,
                RefreshToken = refreshToken.Token
            };

            return tokenData;
        }

        private string RandomStringGenerator(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
                .Select(a => a[random.Next(a.Length)]).ToArray());
        }
    }
}