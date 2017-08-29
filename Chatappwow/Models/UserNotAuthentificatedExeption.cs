using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chatappwow.Models
{
    public class UserNotAuthentificatedExeption : Exception
    {
        public UserNotAuthentificatedExeption()
        {
        }

        public UserNotAuthentificatedExeption(string message) : base(message)
        { }

        public UserNotAuthentificatedExeption(string message, Exception inner) : base(message, inner)
        { }
    }
}