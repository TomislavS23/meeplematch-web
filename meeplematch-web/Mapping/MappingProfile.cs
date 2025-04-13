using AutoMapper;
using meeplematch_web.DTO;
using meeplematch_web.Models;

namespace meeplematch_web.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EventDTO, EventViewModel>().ReverseMap();
            CreateMap<UserDTO, UserViewModel>().ReverseMap();
            CreateMap<PublicUserDTO, UserViewModel>().ReverseMap();
        }
    }
}
