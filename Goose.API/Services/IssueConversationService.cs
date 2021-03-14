﻿using Goose.API.Repositories;
using Goose.API.Utils;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.tickets;
using Goose.Domain.Models.tickets;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface IIssueConversationService
    {
        public Task<IList<IssueConversationDTO>> GetConversationsFromIssueAsync(string issueId);
        public Task<IssueConversationDTO> GetConversationFromIssueAsync(string issueId, string conversationId);
        public Task<IssueConversationDTO> CreateNewIssueConversationAsync(string issueId, IssueConversationDTO conversationItem);
        public Task CreateOrReplaceConversationItemAsync(string issueId, string conversationItemId, IssueConversationDTO conversationItem);
    }

    public class IssueConversationService : IIssueConversationService
    {
        private readonly IIssueRepository _issueRepository;

        public IssueConversationService(IIssueRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }

        /// <summary>
        /// Returns a list of conversation items of the specified issue.
        /// </summary>
        /// <param name="issueId">The issue where the conversation items will be extracted from.</param>
        /// <returns>A list of conversation items from the provided issue.</returns>
        public async Task<IList<IssueConversationDTO>> GetConversationsFromIssueAsync(string issueId)
        {
            var conversationItems = (await _issueRepository.GetIssueByIdAsync(issueId)).ConversationItems;

            // if property is null, set default value => empty array of conversations which do not be saved to the database.
            if (conversationItems is null)
                conversationItems = new List<IssueConversation>();

            // get all conversations from the issue, sorted after the creation date.
            IList<IssueConversation> issueConversations = conversationItems.OrderBy(ci => ci.CreatedAt).ToList();

            // return empty list if null
            if (issueConversations is null)
                return new List<IssueConversationDTO>();

            
            // map poco to dto.
            var issueConversationsDTOs = issueConversations.Select(ic => 
            {
                // TODO: parameters missing.
                //return MapIssueConversationDTO(ic);

                return new IssueConversationDTO(ic, null, null);
            }).ToList();

            // return the mapped conversation items.
            return issueConversationsDTOs;
        }

        //private IssueConversationDTO MapIssueConversationDTO(IssueConversation ic)
        //{
        //    var creator = await _userRepository.GetUserByIdAsync(ic.CreatorUserId);
        //    var creatorDto = new UserDTO(creator);

        //    var requirements = ic.RequirementIds.Select(reqOid => await _issueRepository.GetRequirementByIdAsync(reqOid));
        //    var requirementsDto = requirements.Select(req => new RequirementDTO(req));

        //    return new IssueConversationDTO(ic, creatorDto, requirementsDto);
        //}

        /// <summary>
        /// Returns the requested conversation for the provided issueId.
        /// </summary>
        /// <param name="issueId">The Issue in which the conversation id will be searched for.</param>
        /// <param name="conversationId">The Id of the particular conversation.</param>
        /// <returns>A conversationDTO for the provided issue und conversation id</returns>
        public async Task<IssueConversationDTO> GetConversationFromIssueAsync(string issueId, string conversationId)
        {
            // check if the parsed objectId is not the 000...000 default objectId.
            if (ObjectId.TryParse(conversationId, out ObjectId conversationOid) is false)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Cannot parse conversation string id to a valid object id.");

            var conversationItems = (await _issueRepository.GetIssueByIdAsync(issueId)).ConversationItems;

            if (conversationItems is null)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "No conversation items found.");

            var conversationItem = conversationItems.SingleOrDefault(ci => ci.Id.Equals(conversationOid));

            if (conversationItem is null)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, $"No conversation item with Id={ conversationId } found.");

            // TODO: parameters missing.
            //return MapIssueConversationDTO(ic);
            return new IssueConversationDTO(conversationItem, null, null);
        }

        /// <summary>
        /// Creates a new conversation for the provided issueId.
        /// </summary>
        /// <param name="issueId">The issue which will gets appended with the provided conversation.</param>
        /// <param name="conversationItem">The new conversation item.</param>
        /// <returns>The inserted conversation item.</returns>
        public async Task<IssueConversationDTO> CreateNewIssueConversationAsync(string issueId, IssueConversationDTO conversationItem)
        {
            var issue = await _issueRepository.GetIssueByIdAsync(issueId);
            var conversationItems = issue.ConversationItems;

            // if ConversationItems are null = empty, create a new list, which will gets appended.
            if (conversationItems is null)
                conversationItems = new List<IssueConversation>();

            // TODO: after auth code impl. check if the user who creating this request is identical to the "conversationItem.Creator.Id" -> a client cannot create a conversation for other than himself.

            IssueConversation newConversation = new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = ObjectId.TryParse(conversationItem.Creator.Id, out ObjectId creatorUserId) ? creatorUserId : throw new HttpStatusException(StatusCodes.Status400BadRequest, "The conversation item is missing a valid userId."),
                Data = conversationItem.Data,
                RequirementIds = SelectRequirementsObjectIdsFromDto(conversationItem),
                Type = conversationItem.Type
            };

            // append the conversationItems with the new conversation item.
            conversationItems.Add(newConversation);

            // update the issue and the conversationItems withit.
            await _issueRepository.UpdateAsync(issue);

            // TODO: parameters missing.
            //return MapIssueConversationDTO(ic);
            return new IssueConversationDTO(newConversation, null, null);
        }

        /// <summary>
        /// This Method returns a list of ObjectIds from the Requirments.Id inside of the provided conversationItem.
        /// </summary>
        /// <param name="conversationItem">The conversation which contains the requirements.</param>
        /// <returns>A list of ObjectIds from the requierments.</returns>
        private IList<ObjectId> SelectRequirementsObjectIdsFromDto(IssueConversationDTO conversationItem)
        {
            if (conversationItem.Requirements is null)
                return new List<ObjectId>();

            return conversationItem.Requirements.Select(req => {
                if (ObjectId.TryParse(req.Id, out ObjectId requirmentId) is false)
                    throw new HttpStatusException(StatusCodes.Status400BadRequest, "The requirement does not have a valid object id.");

                return requirmentId;
            }).ToList();
        }

        /// <summary>
        /// Creates or replaces the provided conversation item, based on the id.
        /// </summary>
        /// <param name="issueId">The issue on which the operation will be executed at.</param>
        /// <param name="conversationItemId">The id of the conversation.</param>
        /// <param name="conversationItem">The conversation that will be created or replaced if already existing.</param>
        public async Task CreateOrReplaceConversationItemAsync(string issueId, string conversationItemId, IssueConversationDTO conversationItem)
        {
            if (conversationItemId.Equals(conversationItem.Id) is false)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Id missmatch.");

            var issue = await _issueRepository.GetIssueByIdAsync(issueId);
            var conversationItems = issue.ConversationItems;

            if (ObjectId.TryParse(conversationItem.Id, out ObjectId issueConversationOid) is false) throw new HttpStatusException(StatusCodes.Status400BadRequest, "The conversation item is missing a valid userId.");

            var issueConversationModel = new IssueConversation()
            {
                Id = issueConversationOid,
                CreatorUserId = ObjectId.TryParse(conversationItem.Creator?.Id, out ObjectId creatorUserId) ? creatorUserId : throw new HttpStatusException(StatusCodes.Status400BadRequest, "The conversation item is missing a valid userId."),
                Data = conversationItem.Data,
                RequirementIds = SelectRequirementsObjectIdsFromDto(conversationItem),
                Type = conversationItem.Type
            };

            await _issueRepository.CreateOrUpdateConversationItemAsync(issueId, issueConversationModel);
        }
    }
}