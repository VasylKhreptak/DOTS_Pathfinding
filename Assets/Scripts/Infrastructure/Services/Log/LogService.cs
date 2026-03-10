using Infrastructure.Data.Models.Static.Core;
using Infrastructure.Services.Log.Core;
using UnityEngine;
using LogType = Infrastructure.Services.Log.Core.LogType;

namespace Infrastructure.Services.Log
{
    public class LogService : ILogService
    {
        private readonly IStaticDataModel _staticDataModel;

        public LogService(IStaticDataModel staticDataModel)
        {
            _staticDataModel = staticDataModel;
        }

        public void Log(object message, Object context)
        {
            if (_staticDataModel.Config.LogType.HasFlag(LogType.Info))
                Debug.Log(message, context);
        }

        public void LogWarning(object message, Object context)
        {
            if (_staticDataModel.Config.LogType.HasFlag(LogType.Warning))
                Debug.LogWarning(message, context);
        }

        public void LogError(object message, Object context)
        {
            if (_staticDataModel.Config.LogType.HasFlag(LogType.Error))
                Debug.LogError(message, context);
        }
    }
}