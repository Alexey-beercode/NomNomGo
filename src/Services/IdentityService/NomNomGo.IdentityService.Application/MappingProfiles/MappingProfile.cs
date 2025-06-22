using AutoMapper;
using NomNomGo.IdentityService.Application.DTOs.Response.Authentication;
using NomNomGo.IdentityService.Application.DTOs.Response.Users;
using NomNomGo.IdentityService.Domain.Entities;

namespace NomNomGo.IdentityService.Application.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Authentication mappings
            CreateMap<User, LoginResponse>()
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore());
                
            CreateMap<User, RegisterResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore());
                
            CreateMap<User, CurrentUserResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.Permissions, opt => opt.Ignore());
                
            // User mappings
            CreateMap<User, UpdateProfileResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
                
            CreateMap<User, UserDetailResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Roles, opt => opt.Ignore());
                
            CreateMap<User, UserListItem>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Roles, opt => opt.Ignore());
        }
    }
}