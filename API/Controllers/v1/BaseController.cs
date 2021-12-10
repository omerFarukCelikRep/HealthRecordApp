﻿using AutoMapper;
using DataAccess.Abstract.IConfiguration;
using Entity.Dtos.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class BaseController : ControllerBase
{
    protected readonly IUnitOfWork _unitOfWork;
    public UserManager<IdentityUser> _userManager;
    public readonly IMapper _mapper;

    public BaseController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _mapper = mapper;
    }

    internal Error PopulateError(int code, string message,string type)
    {
        return new Error()
        {
            Code = code,
            Message = message,
            Type = type
        };
    }
}