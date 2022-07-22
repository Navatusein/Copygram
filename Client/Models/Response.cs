using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public enum ResponseType
    {
        Success,
        Error    
    }

    [Serializable]
    public class Response
    {
        public ResponseType Type { get; set; }

        public Command OnCommandResponse { get; set; } = null!;

        public byte[] Data { get; set; } = null!;
    }
}
