using Infrastructure.Services.ID.Core;

namespace Infrastructure.Services.ID
{
    public class IDService : IIDService
    {
        private long _value;

        public long Value => _value++;
    }
}