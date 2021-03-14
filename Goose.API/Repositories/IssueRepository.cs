﻿using Goose.API.Utils.Exceptions;
using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models.tickets;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Repositories
{
    public interface IIssueRepository : IRepository<Issue>
    {
        public Task<Issue> GetIssueByIdAsync(string issueId);
        public Task CreateOrUpdateConversationItemAsync(string issueId, IssueConversation issueConversation);
    }

    public class IssueRepository : Repository<Issue>, IIssueRepository
    {
        public IssueRepository(IDbContext context) : base(context, "issues") { }

        /// <summary>
        /// Returns a issue based on the provided issueId.
        /// </summary>
        /// <param name="issueId"></param>
        /// <returns>A issue for the provided issue id.</returns>
        public async Task<Issue> GetIssueByIdAsync(string issueId)
        {
            // check if the parsed objectId is not the 000...000 default objectId.
            if (ObjectId.TryParse(issueId, out ObjectId issueOid) is false)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Cannot parse issue string id to a valid object id.");

            // fetch the issue that contains the conversation.
            Issue issue = await GetAsync(issueOid);

            // show error if issue is not found.
            if (issue is null)
                throw new HttpStatusException(StatusCodes.Status404NotFound, $"No Issue found with Id={ issueId }.");

            return issue;
        }

        /// <summary>
        /// Creates a conversation item or updates it if existing.
        /// </summary>
        /// <param name="issueId">The issue on which the conversation will be added to.</param>
        /// <param name="issueConversation">The conversation item.</param>
        public async Task CreateOrUpdateConversationItemAsync(string issueId, IssueConversation issueConversation)
        {
            // check if the parsed objectId is not the 000...000 default objectId.
            if (ObjectId.TryParse(issueId, out ObjectId issueOid) is false)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Cannot parse issue string id to a valid object id.");

            var filterDef = Builders<Issue>.Filter;

            var issueFilter = filterDef.Eq(iss => iss.Id, issueOid);
            var distinguishFilter = filterDef.ElemMatch(iss => iss.ConversationItems, (ci) => ci.Id.Equals(issueConversation.Id));
            var push = Builders<Issue>.Update.Push(iss => iss.ConversationItems, issueConversation);

            var result = await _dbCollection.UpdateOneAsync(issueFilter & !distinguishFilter, push);

            // MatchedCount == 0 => filter does not return a conversation item, so we need to update one. If a ci was inserted the MatchedCount would be 1.
            if (result.MatchedCount == 0)
            {
                // [-1] actes as $-Operator from mongodb.
                var update = Builders<Issue>.Update.Set(iss => iss.ConversationItems[-1], issueConversation);
                await _dbCollection.UpdateOneAsync(issueFilter & distinguishFilter, update);
            }
        }
    }
}