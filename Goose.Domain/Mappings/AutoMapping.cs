using AutoMapper;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Tickets;
using Goose.Domain.DTOs.Tickets;
using System;
using System.Collections.Generic;
using System.Text;

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