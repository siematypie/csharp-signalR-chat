using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chatappwow.Models
{
    public class Room
    {
        [Key]
        public string Name { get; set; }
        public ICollection<User> Users { get; set; }
    }
}