using Infrastructure.Data.Models.Persistent.Core;
using Infrastructure.Data.Models.Persistent.Data.Core;

namespace Infrastructure.Data.Models.Persistent
{
    public class PersistentDataModel : IPersistentDataModel
    {
        public PersistentData Data { get; set; }
    }
}