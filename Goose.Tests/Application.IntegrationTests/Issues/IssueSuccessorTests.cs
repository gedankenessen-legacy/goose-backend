using System.Threading.Tasks;
using NUnit.Framework;

namespace Goose.Tests.Application.IntegrationTests.issues
{
    public class IssueSuccessorTests
    {
        [Test]
        public async Task CanAddSuccessor()
        {
        }

        [Test]
        public async Task IssueHasToWaitUntilPredecessorFinishes()
        {
        }

        [Test]
        public async Task CannotSetPredecessorOfAnotherProject()
        {
        }

        [Test]
        public async Task CannotSetParentAsSuccessor()
        {
        }

        [Test]
        public async Task CannotSetChildAsSuccessor()
        {
        }

        
        //TODO cannot add child after specific state
    }
}