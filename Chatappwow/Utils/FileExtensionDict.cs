using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Chatappwow.Models;
using Microsoft.WindowsAzure.Storage.File;

namespace Chatappwow.Utils
{
    public enum SendType {
        File = 0, Image, Audio, Video
    }
    public class FileExtensionDict
    {
        private readonly Dictionary<string, SendType> dict;

        public FileExtensionDict()
        {
            dict = new Dictionary<string, SendType>
            {
                {"image/jpeg", SendType.Image },
                {"image/gif", SendType.Image },
                {"image/png", SendType.Image },
                {"image/svg+xml", SendType.Image },
                {"image/bmp", SendType.Image },
                {"video/mp4", SendType.Video },
                {"video/ogg", SendType.Video },
                {"video/webm", SendType.Video},
                {"audio/aac", SendType.Audio },
                {"audio/mp4", SendType.Audio },
                {"audio/mpeg", SendType.Audio },
                {"audio/ogg", SendType.Audio },
                {"audio/wav", SendType.Audio }
            };
        }

        public SendType GetSendType(File file)
        {
            SendType st;
            dict.TryGetValue(file.Type, out st);
            return st;
        }

    }
}