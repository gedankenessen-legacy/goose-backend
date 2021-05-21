using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Utils.Authentication;
using Goose.Domain.Models;
using Goose.Domain.Models.Companies;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Authorization.Handlers
{
    public class IssueOperationAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Issue>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICompanyRepository _companyRepository;

        public IssueOperationAuthorizationHandler(IProjectRepository projectRepository, ICompanyRepository companyRepository)
        {
            _projectRepository = projectRepository;
            _companyRepository = companyRepository;
        }

        protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Issue issue)
        {
            ObjectId userId = context.User.GetUserId();

            if (context.User is null || issue is null)
                return Task.CompletedTask;

            Project issueProject = await _projectRepository.GetAsync(issue.ProjectId);

            if (issueProject is null)
                return Task.CompletedTask;

            Company issueCompany = await _companyRepository.GetAsync(issueProject.CompanyId);

            if (issueCompany is null)
                return Task.CompletedTask;

            IEnumerable<State> projectStates = issueProject.States;

            if (projectStates is null)
                return Task.CompletedTask;

            State currentIssueState = projectStates.FirstOrDefault(ps => ps.Id.Equals(issue.StateId));

            if (currentIssueState is null)
                return Task.CompletedTask;

            PropertyUser projectUser = issueProject.Users?.FirstOrDefault(usr => usr.UserId.Equals(userId));
            PropertyUser companyUser = issueCompany.Users?.FirstOrDefault(usr => usr.UserId.Equals(userId));

            IList<ObjectId> userRoles = AuthorizationUtils.RolesOfUser(projectUser, companyUser);

            switch (currentIssueState.Phase)
            {
                case State.NegotiationPhase: ValidateRequirmentInNegotiationPhase(context, requirement, userRoles); break;
                case State.ProcessingPhase: ValidateRequirmentInProcessingPhase(context, requirement, userRoles); break;
                case State.ConclusionPhase: ValidateRequirmentInConclusionPhase(context, requirement, userRoles); break;
            }

            return Task.CompletedTask;
        }

        private void ValidateRequirmentInNegotiationPhase(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, IList<ObjectId> userRoles)
        {
            Dictionary<OperationAuthorizationRequirement, Func<IList<ObjectId>, bool>> ValidateUserPermissions = new()
            {
                {
                    IssueOperationRequirments.Create,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.Edit,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.WriteMessage,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.ReadMessages,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ReadonlyEmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.DiscardIssue,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditState,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditStateOfInternal,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.ReadTimeSheets,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ReadonlyEmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditAllTimeSheets,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditOwnTimeSheets,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.AddSubIssue,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.CreateOwnTimeSheets,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.CreateRequirements,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditRequirements,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.AchieveRequirements,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.RemoveRequirements,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                }
            };

            if (ValidateUserPermissions.ContainsKey(requirement) && ValidateUserPermissions[requirement](userRoles))
                context.Succeed(requirement);
        }

        private void ValidateRequirmentInProcessingPhase(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, IList<ObjectId> userRoles)
        {
            // cusomer can only: write messages, discard ticket.
            // employee can only: write messages, edit state, edit own timesheets, add sub-ticket.
            // project leader & company owner have the rights of the employee, additionaly: discard ticket, edit timesheets.
            Dictionary<OperationAuthorizationRequirement, Func<IList<ObjectId>, bool>> ValidateUserPermissions = new()
            {
                {
                    IssueOperationRequirments.Create,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.Edit,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.WriteMessage,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.ReadMessages,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ReadonlyEmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.DiscardIssue,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditState,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditStateOfInternal,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.AddSubIssue,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.ReadTimeSheets,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ReadonlyEmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditOwnTimeSheets,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditAllTimeSheets,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.CreateOwnTimeSheets,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.AchieveRequirements,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
            };

            if (ValidateUserPermissions.ContainsKey(requirement) && ValidateUserPermissions[requirement](userRoles))
                context.Succeed(requirement);
        }

        private void ValidateRequirmentInConclusionPhase(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, IList<ObjectId> userRoles)
        {
            Dictionary<OperationAuthorizationRequirement, Func<IList<ObjectId>, bool>> ValidateUserPermissions = new()
            {
                {
                    IssueOperationRequirments.Create,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.Edit,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.WriteMessage,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.ReadMessages,
                    x => x.Contains(Role.CustomerRole.Id) || x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ReadonlyEmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditState,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditStateOfInternal,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.ReadTimeSheets,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ReadonlyEmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditOwnTimeSheets,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.EditAllTimeSheets,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.CreateOwnTimeSheets,
                    x => x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
                {
                    IssueOperationRequirments.AchieveRequirements,
                    x => x.Contains(Role.EmployeeRole.Id) || x.Contains(Role.ProjectLeaderRole.Id) || x.Contains(Role.CompanyRole.Id)
                },
            };

            if (ValidateUserPermissions.ContainsKey(requirement) && ValidateUserPermissions[requirement](userRoles))
                context.Succeed(requirement);
        }
    }
}
