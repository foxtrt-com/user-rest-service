using AutoMapper;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Profiles;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        // Source to Target
        CreateMap<User, UserReadDto>();
        CreateMap<UserCreateDto, User>();
        CreateMap<UserReadDto, UserPublishedDto>();
    }
}
