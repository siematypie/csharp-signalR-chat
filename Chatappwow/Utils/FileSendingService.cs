using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Chatappwow.Hubs;
using Chatappwow.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using File = Chatappwow.Models.File;


namespace Chatappwow.Utils
{

    public class FileSendingService
    {
        private static readonly Lazy<FileSendingService> _instance =
            new Lazy<FileSendingService>(() => 
            new FileSendingService(GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients, CloudConfigurationManager.GetSetting("StorageConnectionString")));

        private readonly FileExtensionDict _extdict;
        private readonly CloudBlobContainer _container;
        private readonly IHubConnectionContext<dynamic> _clients;
        public static FileSendingService Instance => _instance.Value;

        private FileSendingService(IHubConnectionContext<dynamic> clients, string azureConnecrionString)
        {
            _clients = clients;
            _extdict = new FileExtensionDict();
            var storageAccount = CloudStorageAccount.Parse(azureConnecrionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
//            AddCorsRule(blobClient);
            var container = blobClient.GetContainerReference("chatcontainer");
            container.CreateIfNotExists();
            container.SetPermissions(
                new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            _container = container;

        }

        private void AddCorsRule(CloudBlobClient blobClient)
        {
            var serviceProperties = blobClient.GetServiceProperties();
            var corsSettings = serviceProperties.Cors;

            var corsRule = new CorsRule()
            {
                AllowedHeaders = new List<string> { "x-ms-*", "content-type", "accept" },
                AllowedMethods = CorsHttpMethods.Put,//Since we'll only be calling Put Blob, let's just allow PUT verb
                AllowedOrigins = new List<string> { "*" },//This is the URL of our application.
                MaxAgeInSeconds = 1 * 60 * 60,//Let the browswer cache it for an hour
            };
            corsSettings.CorsRules.Add(corsRule);
            blobClient.SetServiceProperties(serviceProperties);
        }

        public void SendFileTokenToUser(File file)
        {
//            var extension = Path.GetExtension(file.Name);
//            var newFileName = $@"{DateTime.Now.Ticks}{extension}";
            var blockBlob = _container.GetBlockBlobReference(file.Name);
            var sasPolicy = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(15),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write
            };
            var sasBlobToken = blockBlob.GetSharedAccessSignature(sasPolicy);

            _clients.Client(file.SenderId).sasAquired(blockBlob.Uri, sasBlobToken);
        }

        public void UploadFile(File file)
        {
            
            if (!file.SizeValid)
            {
                _clients.Client(file.SenderId).FileTooBig(File.MaxSize.ToString());
            }
            else if (file.Url == null)
            {
                SendFileTokenToUser(file);
            }
            else
            {
                BroadcastFile(file);
            }
        }

        private void BroadcastFile(File file)
        {
            using (var db = new UserContext())
            {
                db.Rooms.Attach(file.SenderRoom);
                db.Messages.Add(new Message(file));
                db.SaveChanges();
            }
            var sendType = _extdict.GetSendType(file);
            var roomName = file.SenderRoom.Name;
            var fileInfo = new { name = file.Name, sender = file.SenderName, url = file.Url, size = file.Size, type = file.Type };
            switch (sendType)
            {
                case SendType.Image:
                    _clients.Group(roomName).BroadcastImage(fileInfo);
                    break;
                case SendType.Video:
                    _clients.Group(roomName).BroadcastVideo(fileInfo);
                    break;
                case SendType.Audio:
                    _clients.Group(roomName).BroadcastAudio(fileInfo);
                    break;
                case SendType.File:
                    _clients.Group(roomName).BroadcastFile(fileInfo);
                    break;
            }
        }
    }
}