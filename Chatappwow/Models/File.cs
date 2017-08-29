using System;

namespace Chatappwow.Models
{


    public class File
    {
        public const long MaxSize = 20971520; // 20mb
        private long _size;
        public bool SizeValid { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string SenderName { get; set; }
        public string SenderId { get; set; }
        public Room SenderRoom { get; set; }

        public long Size
        {
            get { return _size; }
            set
            {
                _size = value;
                SizeValid = value <= MaxSize;
            }
        }
     
        public string Type { get; set; }
        public string Extension { get; set; }

        public void AddUserInfo(User usr, string connId)
        {
            SenderName = usr.UserName;
            SenderRoom = usr.Room;
            SenderId = connId;
        }

    }
}