using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Requirements
{
    public static class IssueOperationRequirments
    {
        public readonly static OperationAuthorizationRequirement Create = new() { Name = nameof(Create) };
        public readonly static OperationAuthorizationRequirement Edit = new() { Name = nameof(Edit) };

        public readonly static OperationAuthorizationRequirement WriteMessage = new() { Name = nameof(WriteMessage) };
        public readonly static OperationAuthorizationRequirement ReadMessages = new() { Name = nameof(ReadMessages) };
        public readonly static OperationAuthorizationRequirement DiscardTicket = new() { Name = nameof(DiscardTicket) };
        public readonly static OperationAuthorizationRequirement EditState = new() { Name = nameof(EditState) };
        public readonly static OperationAuthorizationRequirement EditOwnTimeSheets = new() { Name = nameof(EditOwnTimeSheets) };
        public readonly static OperationAuthorizationRequirement EditAllTimeSheets = new() { Name = nameof(EditAllTimeSheets) };
        public readonly static OperationAuthorizationRequirement AddSubTicket = new() { Name = nameof(AddSubTicket) };
    }
}
