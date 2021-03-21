using AutoMapper;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.identity;
using Goose.Domain.Models.tickets;

namespace Goose.Domain.Mapping
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<Issue, IssueResponseDTO>().ReverseMap();
            CreateMap<User, UserDTO>();
            CreateMap<TimeSheet, IssueTimeSheetDTO>()
                .ForMember(it => it.User, o => o.Ignore());
            CreateMap<IssueTimeSheetDTO, TimeSheet>()
                .ForMember(it => it.UserId, o => o.MapFrom(it => it.User.Id));
        }
    }
}