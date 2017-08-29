using System.ComponentModel.DataAnnotations;

namespace Chatappwow.Models
{
    public class Connection
    {
        public string ConnectionId { get; set; }
        [Required]
        public User User { get; set; }
    }
}