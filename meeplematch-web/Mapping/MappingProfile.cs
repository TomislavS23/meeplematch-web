using AutoMapper;

namespace meeplematch_web.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<meeplematch_api.Model.Event, meeplematch_web.Models.EventViewModel>();
            CreateMap<meeplematch_web.Models.EventViewModel, meeplematch_api.Model.Event>();

            CreateMap<meeplematch_api.Model.Event, meeplematch_api.DTO.EventDTO>();
            CreateMap<meeplematch_api.DTO.EventDTO, meeplematch_api.Model.Event>();

            CreateMap<meeplematch_api.DTO.EventDTO, meeplematch_web.Models.EventViewModel>();
            CreateMap<meeplematch_web.Models.EventViewModel, meeplematch_api.DTO.EventDTO>();
        }
    }
}
