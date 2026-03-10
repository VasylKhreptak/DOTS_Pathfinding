using Infrastructure.Data.Models.Static.Data;

namespace Infrastructure.Data.Models.Static.Core
{
    public interface IStaticDataModel
    {
        public Config Config { get; }
        public Balance Balance { get; }
    }
}