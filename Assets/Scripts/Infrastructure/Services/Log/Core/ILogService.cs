using UnityEngine;

namespace Infrastructure.Services.Log.Core
{
    public interface ILogService
    {
        public void Log(object message, Object context = null);

        public void LogWarning(object message, Object context = null);

        public void LogError(object message, Object context = null);
    }
}