using System;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Chatappwow.Models
{
    [Flags]
    public enum IncludeFields
    {
       Room = 1,
       Connections = 2,
       All = Room | Connections
    }

    public class Identity
    {
        private string _name;

        public string Name
        {
            get
            {
              return _name;
            }
            set { _name = HttpUtility.HtmlEncode(value); }

        }

    public string Token { get; set; }

        public bool IsValid()
        {
            return Name != null && Token != null;
        }

        public bool TryGetUser(UserContext db, IncludeFields opt, out User user)
        {
            user = null;
            if (!IsValid()) return false;
            
            IQueryable<User> table = db.Users;
            if (opt.HasFlag(IncludeFields.Room))
            {
                table = table.Include(u => u.Room);
            }
            if (opt.HasFlag(IncludeFields.Connections))
            {
                table = table.Include(u => u.Connections);
            }
            user = table.SingleOrDefault(u => u.UserName == Name && u.Token == Token);
            return user != null;
        }
    }
}