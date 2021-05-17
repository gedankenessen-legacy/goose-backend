using Goose.Data.Context;
using Goose.Data.Repository;
using Goose.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
     
    }

    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(IDbContext context) : base(context, "messages")
        {

        }
    }
}
