using AutoMapper;
using Entity.Concrete;
using Entity.Dtos.Incoming;
using Entity.Dtos.Outgoing.Profile;
using Entity.Enums;

namespace API.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDto, AppUser>()
                .ForMember(
                    destination => destination.FirstName,
                    from => from.MapFrom(a => $"{a.FirstName}")
                )
                .ForMember(
                    destination => destination.LastName,
                    from => from.MapFrom(a => $"{a.LastName}")
                )
                .ForMember(
                    destination => destination.Email,
                    from => from.MapFrom(a => $"{a.Email}")
                )
                .ForMember(
                    destination => destination.Phone,
                    from => from.MapFrom(a => $"{a.Phone}")
                )
                .ForMember(
                    destination => destination.DateOfBirth,
                    from => from.MapFrom(a => Convert.ToDateTime(a.DateOfBirth))
                )
                .ForMember(
                    destination => destination.Country,
                    from => from.MapFrom(a => $"{a.Country}")
                )
                .ForMember(
                    destination => destination.Status,
                    from => from.MapFrom(a => Status.Added)
                );

            CreateMap<AppUser, ProfileDto>()
                .ForMember(
                    destination => destination.Country,
                    from => from.MapFrom(a => $"{a.Country}")
                )
                .ForMember(
                    destination => destination.DateOfBirth,
                    from => from.MapFrom(a => $"{a.DateOfBirth.ToShortDateString()}")
                )
                .ForMember(
                    destination => destination.Email,
                    from => from.MapFrom(a => $"{a.Email}")
                )
                .ForMember(
                    destination => destination.FirstName,
                    from => from.MapFrom(a => $"{a.FirstName}")
                )
                .ForMember(
                    destination => destination.LastName,
                    from => from.MapFrom(a => $"{a.LastName}")
                )
                .ForMember(
                    destination => destination.Phone,
                    from => from.MapFrom(a => $"{a.Phone}")
                );
        }
    }
}
