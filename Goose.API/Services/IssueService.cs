using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Goose.API.Repositories;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.tickets;

namespace Goose.API.Services
{
    public interface IIssueService
    {
        Task<IList<IssueDTOSimple>> GetAllIssues();
    }

    public class IssueService : IIssueService
    {
        private readonly IIssueRepository _issueRepo;
        private readonly IMapper _mapper;

        public IssueService(IIssueRepository issueRepo, IMapper mapper)
        {
            _issueRepo = issueRepo;
            _mapper = mapper;
        }

        public async Task<IList<IssueDTOSimple>> GetAllIssues()
        {
            var res = await _issueRepo.GetAsync();
            return _mapper.Map<List<IssueDTOSimple>>(res);
        }
    }
}