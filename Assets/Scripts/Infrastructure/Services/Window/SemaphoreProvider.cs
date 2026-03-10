using System.Threading;

namespace Infrastructure.Services.Window
{
    public class SemaphoreProvider
    {
        public readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
    }
}