using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Utils.Exceptions;
using Goose.Domain.Models.Issues;
using MongoDB.Bson;

namespace Goose.API.Services.issues
{
    public interface IIssueHelper
    {
        public Task CanAddChild(Issue parent, Issue other);
        public Task CanAddPredecessor(Issue successor, Issue other);
        public Task<IList<Issue>> GetChildrenRecursive(Issue issue);
    }

    public class IssueHelper : IIssueHelper
    {
        private readonly IIssueRepository _issueRepository;

        public IssueHelper(IIssueRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }


        public async Task CanAddAssociation(IList<Issue> projectIssues, Issue issue, Issue other)
        {
            //TODO mom. wird ticket mehrmals abgearbeitet
            Queue<Issue> tickets = new Queue<Issue>();

            //fügt sich selber und alle Kinder hinzu. Die Kinder werden hinzugefügt, da der Vorgänger eines Obertickets der Vorgänger aller 
            //Untertickets ist
            tickets.Enqueue(issue);
            foreach (var ticket in GetChildrenRecursive(projectIssues, issue))
                tickets.Enqueue(ticket);

            /*
             * Iteriert durch jedes Ticket bis die Queue leer ist oder element.Equals(other) true ist. Bei jeder Iteration werden Eltern
             * und alle Nachfolger (auch indirekte) der Queue hinzugefügt
             */
            while (tickets.Count > 0)
            {
                var element = tickets.Dequeue();
                if (element == null) continue;
                if (element.Id.Equals(other.Id))
                    throw new HttpStatusException(400, $"An endless loop would occur if {issue.Id} and {other.Id} were to be associated");

                if (element.ParentIssueId is ObjectId parentId) tickets.Enqueue(GetIssue(projectIssues, parentId));
                foreach (var ticket in GetSuccessorsAdvanced(projectIssues, element))
                    tickets.Enqueue(ticket);
            }
        }

        public async Task CanAddChild(Issue parent, Issue other)
        {
            if (parent.ProjectId != other.ProjectId)
                throw new HttpStatusException(400, $"Issues do not belong to same project, " +
                                                   $"Issue {parent.Id} belongs to project {parent.ProjectId}, issue {other.Id} belongs to {other.ProjectId}");
            if (parent.Id == other.Id)
                throw new HttpStatusException(400, "Cannot associate an issue with itself");

            var projectIssues = await _issueRepository.GetAllOfProjectAsync(parent.ProjectId);

            if (GetRootIssue(projectIssues, parent) == GetRootIssue(projectIssues, other))
                throw new HttpStatusException(400,
                    $"{parent.Id} and {other.Id} belong to the same hierarchy. Cannot associate issue or an endless loop would occur");

            var children = GetChildren(projectIssues, parent);
            if (parent.IssueDetail.ExpectedTime < other.IssueDetail.ExpectedTime + children.Sum(it => it.IssueDetail.ExpectedTime))
                throw new HttpStatusException(400, "total expected time would be larger than expected time of parent");
            
            await CanAddAssociation(projectIssues, parent, other);
        }

        public async Task<IList<Issue>> GetChildrenRecursive(Issue issue)
        {
            var projectIssues = await _issueRepository.GetAllOfProjectAsync(issue.ProjectId);
            return GetChildrenRecursive(projectIssues, issue);
        }

        public async Task CanAddPredecessor(Issue successor, Issue other)
        {
            if (successor.ProjectId != other.ProjectId)
                throw new HttpStatusException(400, $"Issues do not belong to same project, " +
                                                   $"Issue {successor.Id} belongs to project {successor.ProjectId}, issue {other.Id} belongs to {other.ProjectId}");
            if (successor.Id == other.Id)
                throw new HttpStatusException(400, "Cannot associate an issue with itself");

            var projectIssues = await _issueRepository.GetAllOfProjectAsync(successor.ProjectId);
            if (IsChildOf(projectIssues, successor, other) || IsChildOf(projectIssues, other, successor))
                throw new HttpStatusException(400,
                    $"{successor.Id} or {other.Id} are the parent of the other issue. Cannot associate issue or an endless loop would occur");

            await CanAddAssociation(projectIssues, successor, other);
        }

        private Issue GetRootIssue(IList<Issue> projectIssues, Issue issue)
        {
            var parent = issue;
            while (parent.ParentIssueId is ObjectId parentId)
                parent = GetIssue(projectIssues, parentId);
            return parent;
        }

        private bool IsChildOf(IList<Issue> projectIssues, Issue parent, Issue other)
        {
            var children = GetChildrenRecursive(projectIssues, parent);
            return children.FirstOrDefault(it => it.Id == other.Id) != null;
        }

        private IList<Issue> GetChildrenRecursive(IList<Issue> projectIssues, Issue issue)
        {
            var children = new List<Issue>(GetChildren(projectIssues, issue));
            children.AddRange(children.SelectMany(it => GetChildrenRecursive(projectIssues, it)));
            return children;
        }

        private IList<Issue> GetChildren(IList<Issue> projectIssues, Issue issue)
        {
            return new List<Issue>(issue.ChildrenIssueIds.Select(it => GetIssue(projectIssues, it)));
        }

        /**
         * Returns Successors and it's children
         */
        private IList<Issue> GetSuccessorsAdvanced(IList<Issue> projectIssues, Issue issue)
        {
            var result = new List<Issue>(issue.SuccessorIssueIds.Select(it => GetIssue(projectIssues, it)));
            result.AddRange(result.SelectMany(child => GetChildrenRecursive(projectIssues, child)));
            return result;
        }

        private Issue GetIssue(IList<Issue> projectIssues, ObjectId issueId)
        {
            return projectIssues.First(it => it.Id == issueId);
        }
    }
}