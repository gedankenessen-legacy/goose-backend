using AutoMapper;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.tickets;

namespace Goose.Domain.Mapping
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<Issue, IssueRequestDTO>().ReverseMap();
            CreateMap<Issue, IssueResponseDTO>().ReverseMap();
        }
    }
}