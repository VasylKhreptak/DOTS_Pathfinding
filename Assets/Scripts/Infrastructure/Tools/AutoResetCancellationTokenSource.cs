using System.Threading;

namespace Infrastructure.Tools
{
    public class AutoResetCancellationTokenSource
    {
        private CancellationTokenSource _cts;

        public AutoResetCancellationTokenSource()
        {
            _cts = new CancellationTokenSource();
        }

        private bool _cancelled;

        public CancellationToken Token
        {
            get
            {
                if (_cancelled)
                {
                    _cts = new CancellationTokenSource();
                    _cancelled = false;
                }

                return _cts.Token;
            }
        }

        public void Cancel()
        {
            if (_cancelled)
                return;

            _cts.Cancel();
            _cts.Dispose();
            _cancelled = true;
        }
    }
}