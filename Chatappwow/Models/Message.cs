using System;
using System.Data.Entity.Migrations.Sql;
using Newtonsoft.Json;

namespace Chatappwow.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public string UserName { get; set; }

        [JsonIgnore]
        public Room Room { get; set; }
        public long Size { get; set; }
        public string Text { get; set; }
        public DateTime SentTime { get; set; }

        public Message()
        {   
        }

        public Message(User usr, string msg)
        {
            UserName = usr.UserName;
            Room = usr.Room;
            Text = msg;
            SentTime = DateTime.Now;
        }

        public Message(File file)
        {
            UserName = file.SenderName;
            Room = file.SenderRoom;
            Size = file.Size;
            Text = file.Url;
            SentTime = DateTime.Now;;
        }
    }
}