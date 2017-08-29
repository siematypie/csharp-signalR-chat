using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

using System.Linq;
using System.Web;

namespace Chatappwow.Models
{
    public class User
    {
        [Key]
        public string UserName { get; set; }
        public ICollection<Connection> Connections { get; set; }
        public string Token { get; set; }
        [Required]
        public Room Room { get; set; }

        public User()
        {
           
        }

        public User(string name, Connection con, Room room)
        {
            UserName = name;
            Room = room;
            Connections = new List<Connection>{con};
            Token = GenerateToken();
        }

        public static string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }     
    }
}