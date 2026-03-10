using Infrastructure.Data.Models.Persistent.Data.Core;

namespace Infrastructure.Data.Models.Persistent.Core
{
    public interface IPersistentDataModel
    {
        public PersistentData Data { get; set; }
    }
}