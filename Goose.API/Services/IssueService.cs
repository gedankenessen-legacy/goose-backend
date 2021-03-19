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
        Task<IList<IssueResponseDTO>> GetAll();
        Task<IList<IssueResponseDTO>> GetAllOfProject(ObjectId projectId);
        public Task<IssueResponseDTO> Get(ObjectId id);
        public Task<IssueResponseDTO> GetOfProject(ObjectId projectId, ObjectId id);
        public Task<IssueResponseDTO> Create(IssueRequestDTO issueRequestDto);
        public Task<IssueResponseDTO> Update(IssueRequestDTO issueRequest, ObjectId id);
        public Task<bool> Delete(ObjectId id);
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

        public async Task<IList<IssueResponseDTO>> GetAll()
        {
            return _mapper.Map<List<IssueResponseDTO>>(await _issueRepo.GetAsync());
        }


        public async Task<IList<IssueResponseDTO>> GetAllOfProject(ObjectId projectId)
        {
            return _mapper.Map<List<IssueResponseDTO>>(await _issueRepo.GetAllOfProjectAsync(projectId));
        }

        public async Task<IssueResponseDTO> Get(ObjectId id)
        {
            return _mapper.Map<IssueResponseDTO>(await _issueRepo.GetAsync(id));
        }

        public async Task<IssueResponseDTO> GetOfProject(ObjectId projectId, ObjectId id)
        {
            return _mapper.Map<IssueResponseDTO>(await _issueRepo.GetOfProjectAsync(projectId, id));
        }

        public async Task<IssueResponseDTO> Create(IssueRequestDTO issueRequestDto)
        {
            var issue = _mapper.Map<Issue>(issueRequestDto);
            await _issueRepo.CreateAsync(issue);
            //TODO wie bekomme ich das issue mit neuer id zurück
            return null;
        }

        public async Task<IssueResponseDTO> Update(IssueRequestDTO issueRequest, ObjectId id)
        {
            await _issueRepo.UpdateAsync(_mapper.Map<Issue>(issueRequest));
            //TODO manche felder dürfen nicht geupdated werden
            return await Get(id);
        }

        public async Task<bool> Delete(ObjectId id)
        {
            return (await _issueRepo.DeleteAsync(id)).DeletedCount > 0;
        }
    }
}