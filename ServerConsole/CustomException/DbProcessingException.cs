using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ServerConsole.CustomException
{
    [Serializable]
    internal class DbProcessingException : Exception
    {
        public DbProcessingException()
        { 
        }

        public DbProcessingException(string message) : base(message)
        { 
        }

        public DbProcessingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
