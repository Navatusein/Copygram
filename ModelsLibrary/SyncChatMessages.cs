using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    [Serializable]
    public class SyncChatMessages
    {
        public int MessageId { get; set; }

        public int ChatId { get; set; }

        public int MessageCount { get; set; }
    }
}
