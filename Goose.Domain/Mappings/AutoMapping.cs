using AutoMapper;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.identity;
using Goose.Domain.Models.tickets;
using Goose.Domain.DTOs.tickets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Goose.Domain.Mapping
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<User, UserDTO>();
            CreateMap<TimeSheet, IssueTimeSheetDTO>()
                .ForMember(it => it.User, o => o.Ignore());
            CreateMap<IssueTimeSheetDTO, TimeSheet>()
                .ForMember(it => it.UserId, o => o.MapFrom(it => it.User.Id));
        }
    }
}