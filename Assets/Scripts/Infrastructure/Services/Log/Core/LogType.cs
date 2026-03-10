using System;

namespace Infrastructure.Services.Log.Core
{
    [Flags]
    public enum LogType
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 4
    }
}