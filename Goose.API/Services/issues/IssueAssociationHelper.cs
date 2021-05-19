using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Utils.Exceptions;
using Goose.Domain.Models.Issues;
using MongoDB.Bson;

namespace Goose.API.Services.issues
{
    public interface IIssueAssociationHelper
    {
        public Task CanAddAssociation(Issue issue, Issue other);
    }

    public class IssueAssociationHelper : IIssueAssociationHelper
    {
        private readonly IIssueRepository _issueRepository;


        public async Task CanAddAssociation(Issue issue, Issue other)
        {
            if (issue.ProjectId != other.ProjectId)
                throw new HttpStatusException(400, $"Issues do not belong to same project, " +
                                                   $"Issue {issue.Id} belongs to project {issue.ProjectId}, issue {other.Id} belongs to {other.ProjectId}");
            //Falls A Ein Ober-/Unter-ticket von B ist 
            var projectIssues = await _issueRepository.GetAllOfProjectAsync(issue.ProjectId);
            if (IsChildOf(projectIssues, issue, other) || IsChildOf(projectIssues, other, issue))
                throw new HttpStatusException(400,
                    $"{issue.Id} or {other.Id} the parent of the other issue. Cannot associate issue or an enless lopp would occur");

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

        private bool IsChildOf(IList<Issue> projectIssues, Issue parent, Issue other)
        {
            var children = GetChildrenRecursive(projectIssues, parent);
            return children.FirstOrDefault(it => it.Id == other.Id) != null;
        }

        private IList<Issue> GetChildrenRecursive(IList<Issue> projectIssues, Issue issue)
        {
            var children = new List<Issue>(issue.ChildrenIssueIds.Select(it => GetIssue(projectIssues, it)));
            children.AddRange(children.SelectMany(it => GetChildrenRecursive(projectIssues, it)));
            return children;
        }

        /**
         * Returns Successors and it's children
         */
        public IList<Issue> GetSuccessorsAdvanced(IList<Issue> projectIssues, Issue issue)
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