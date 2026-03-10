using Plugins.Banks.Integer;

namespace Infrastructure.Data.Models.Persistent.Data
{
    public class PlayerData
    {
        public IntegerBank Coins = new IntegerBank(0);
    }
}