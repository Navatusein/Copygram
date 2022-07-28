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
        BadPasswordOrLogin,
        LoginBusy,
        NicknameBusy,
        UnknownCommand,
        UnknownUser,
        UnknownCommandArguments
    }

    [Serializable]
    public class Error
    {
        public KnownErrors Type { get; set; }

        public string Text { get; set; } = null!;
    }
}
