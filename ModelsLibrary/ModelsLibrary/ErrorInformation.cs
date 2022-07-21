using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    public enum KnownErrors
    {
        UnknownError,
        OutOfSync,
        SecondClient,
        BadPasswordOrLogin
    }

    [Serializable]
    public class ErrorInformation
    {
        public KnownErrors Type { get; set; }

        public string Text { get; set; } = null!;
    }
}
