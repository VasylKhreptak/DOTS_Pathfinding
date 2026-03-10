using System;
using Plugins.StompyRobot.SRDebugger.Attributes;
using VContainer.Unity;

namespace DebuggerOptions.Core
{
    public class BaseOptions : IInitializable, IDisposable
    {
        private bool _initialized;
        private bool _disposed;

        [Ignore]
        public void Initialize()
        {
            if (_initialized)
                return;

            SRDebug.Instance.AddOptionContainer(this);

            _initialized = true;
            _disposed = false;
        }

        [Ignore]
        public void Dispose()
        {
            if (_disposed)
                return;

            SRDebug.Instance?.RemoveOptionContainer(this);

            _initialized = false;
        }
    }
}