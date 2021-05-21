using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Requirements
{
    public static class IssueOperationRequirments
    {
        #region Issue
        public readonly static OperationAuthorizationRequirement Create = new() { Name = nameof(Create) };
        public readonly static OperationAuthorizationRequirement Edit = new() { Name = nameof(Edit) };
        public readonly static OperationAuthorizationRequirement EditState = new() { Name = nameof(EditState) };
        public readonly static OperationAuthorizationRequirement EditStateOfInternal = new() { Name = nameof(EditStateOfInternal) };
        public readonly static OperationAuthorizationRequirement DiscardIssue = new() { Name = nameof(DiscardIssue) };
        public readonly static OperationAuthorizationRequirement AddSubIssue = new() { Name = nameof(AddSubIssue) };
        #endregion

        #region Issue.Message
        public readonly static OperationAuthorizationRequirement WriteMessage = new() { Name = nameof(WriteMessage) };
        public readonly static OperationAuthorizationRequirement ReadMessages = new() { Name = nameof(ReadMessages) };
        #endregion

        #region Issue.TimeSheet
        public readonly static OperationAuthorizationRequirement CreateOwnTimeSheets = new() { Name = nameof(CreateOwnTimeSheets) };
        public readonly static OperationAuthorizationRequirement EditOwnTimeSheets = new() { Name = nameof(EditOwnTimeSheets) };
        public readonly static OperationAuthorizationRequirement EditAllTimeSheets = new() { Name = nameof(EditAllTimeSheets) };
        #endregion

        #region Issue.Requrements
        public readonly static OperationAuthorizationRequirement CreateRequirements = new() { Name = nameof(CreateRequirements) };
        public readonly static OperationAuthorizationRequirement EditRequirements = new() { Name = nameof(EditRequirements) };
        public readonly static OperationAuthorizationRequirement AchieveRequirements = new() { Name = nameof(AchieveRequirements) };
        public readonly static OperationAuthorizationRequirement RemoveRequirements = new() { Name = nameof(RemoveRequirements) };
        #endregion
    }
}
