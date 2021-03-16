using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Goose.API.Repositories;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.tickets;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Goose.API.Services
{
    public interface IIssueService
    {
        Task<IList<IssueDTO>> GetAll();
        Task<IList<IssueDTO>> GetAllOfProject(ObjectId projectId);
        public Task<IssueDTO> Get(ObjectId id);
        public Task<IssueDTO> GetOfProject(ObjectId projectId, ObjectId id);
        public Task<IssueDTO> Create(IssueDTO issue);
        public Task<IssueDTO> Update(IssueDTO issue);

        public Task<IssueDTO> CreateOrUpdate(IssueDTO issue);
        public Task<DeleteResult> Delete(ObjectId id);
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

        public async Task<IList<IssueDTO>> GetAll()
        {
            return _mapper.Map<List<IssueDTO>>(await _issueRepo.GetAsync());
        }


        public async Task<IList<IssueDTO>> GetAllOfProject(ObjectId projectId)
        {
            return _mapper.Map<List<IssueDTO>>(await _issueRepo.GetAllOfProjectAsync(projectId));
        }

        public async Task<IssueDTO> Get(ObjectId id)
        {
            return _mapper.Map<IssueDTO>(await _issueRepo.GetAsync(id));
        }

        public async Task<IssueDTO> GetOfProject(ObjectId projectId, ObjectId id)
        {
            return _mapper.Map<IssueDTO>(await _issueRepo.GetOfProjectAsync(projectId, id));
        }

        public async Task<IssueDTO> Create(IssueDTO issue)
        {
            await _issueRepo.CreateAsync(_mapper.Map<Issue>(issue));
            return issue;
        }

        public async Task<IssueDTO> Update(IssueDTO issue)
        {
            await _issueRepo.UpdateAsync(_mapper.Map<Issue>(issue));
            return issue;
        }

        public async Task<IssueDTO> CreateOrUpdate(IssueDTO issue)
        {
            var exists = await Get(issue.Id) != null;
            if (exists) return await Update(issue);
            return await Create(issue);
        }

        public async Task<DeleteResult> Delete(ObjectId id)
        {
            return await _issueRepo.DeleteAsync(id);
        }
    }
}