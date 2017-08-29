using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc.Routing.Constraints;
using System.Web.UI;
using Chatappwow.Models;
using Chatappwow.Utils;
using Microsoft.AspNet.SignalR;

namespace Chatappwow.Hubs
{
    public class ChatHub : Hub
    {
        private const string DefaultRoom = "General";
        
        public void Send(string message, Identity identity)
        {
            using (var db = new UserContext())
            {
                User usr;
                if (identity.TryGetUser(db, IncludeFields.Room, out usr))
                {
                    message = HttpUtility.HtmlEncode(message);
                    db.Messages.Add(new Message(usr, message));
                    db.SaveChanges();
                    Clients.Group(usr.Room.Name).broadcastMessage(usr.UserName, message);
                }
            }
        }


        public void GetOlderMessages(string date, Identity identity)
        {
            using (var db = new UserContext())
            {
                DateTime myDate;
                User usr;
                if (DateTime.TryParse(date, out myDate) && identity.TryGetUser(db, IncludeFields.Room, out usr))
                {
                    var olderMessages = db.Messages.Where(m => m.Room.Name == usr.Room.Name && m.SentTime < myDate).OrderByDescending(m => m.SentTime).
                        Take(15).ToList();
                    Clients.Caller.olderMessages(olderMessages);
                }
            }

        }

        public async Task Notify(Identity identity)
        {
            if (!identity.IsValid())
            {
                Clients.Caller.differentName();
                return;
            }
            User user;
            bool newUserWasAdded;
           
            using (var db = new UserContext())
            {
                newUserWasAdded = TryAddUser(db, identity.Name, out user);
                if (!newUserWasAdded)
                {
                    if (user.Token != identity.Token)
                    {
                        Clients.Caller.differentName();
                        return;
                    }
                    user.Connections.Add(new Connection{ConnectionId = Context.ConnectionId});
                    db.SaveChanges();
                }
                var lastmessages = db.Messages.Where(m => m.Room.Name == user.Room.Name).OrderByDescending(m => m.SentTime).
                    Take(15).ToList();
                Clients.Caller.lastMessages(lastmessages);

            }

            var userNames = user.Room.Users.Select(r => r.UserName).ToList();
            await Groups.Add(Context.ConnectionId, DefaultRoom);
            if (newUserWasAdded)
            {
                Clients.OthersInGroup(DefaultRoom).addUser(user.UserName);
                Clients.Group(DefaultRoom).enters(user.UserName);
            }
            Clients.Caller.joinedToChat(user.Token, userNames);
            await Groups.Add(Context.ConnectionId, user.Token);
        }

        private bool TryAddUser(UserContext db, string name, out User user)
        {
            user = db.Users.Include(u => u.Connections).Include(u => u.Room.Users).
                SingleOrDefault(u => u.UserName == name);
            if (user != null) return false;
            var room = db.Rooms.Include(r => r.Users)
                .FirstOrDefault(r => r.Name == DefaultRoom);
            
            user = new User(name, new Connection { ConnectionId = Context.ConnectionId }, room);
            db.Users.Add(user);
            db.SaveChanges();
            return true;
        }

        private void Disconnect(bool allConnections = false)
        {
             using (var db = new UserContext())
            {
                var conn = db.Connections.Include(u => u.User.Connections).Include(u => u.User.Room.Users)
                    .SingleOrDefault(u => u.ConnectionId == Context.ConnectionId);
                if (conn == null) return;
                var usr = conn.User;
                if (allConnections || usr.Connections.Count == 1)
                {
                    var room = usr.Room;
                    if (room.Name != DefaultRoom && room.Users.Count == 1)
                        db.Rooms.Remove(usr.Room);
                    else
                    {
                        if (allConnections)
                        {
                            Clients.Group(usr.Token).LoggedOut();
                            foreach (var con in usr.Connections)
                            {
                                Groups.Remove(con.ConnectionId, usr.Token);
                                Groups.Remove(con.ConnectionId, room.Name);
                            }
                        }
                        db.Users.Remove(usr);
                        Clients.Group(room.Name).disconnected(usr.UserName);
                    }
                }
                else
                {
                    db.Connections.Remove(conn);
                }
                db.SaveChanges();
            }
            
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Disconnect();
            return base.OnDisconnected(stopCalled);
        }

        public void LogOut()
        {
            Disconnect(true);
        }

        public void SendFile(File file, string name, string token)
        {   
            if (token == null || name == null ) return;
            User usr;
            using (var db = new UserContext())
            {
                usr = db.Users.Include(u => u.Room).SingleOrDefault(u => u.UserName == name && u.Token == token);
            }
            if (usr == null) return;
            file.AddUserInfo(usr, Context.ConnectionId);
            FileSendingService.Instance.UploadFile(file);
        }
       
    }

   
}