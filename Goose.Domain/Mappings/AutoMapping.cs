using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.tickets;

namespace Goose.Domain.Mapping
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<Issue, IssueDTOSimple>();
            CreateMap<IssueDTOSimple, Issue>();
        }
    }
}
