using Infrastructure.Data.Models.Static.Core;
using Infrastructure.Data.Models.Static.Data;
using UnityEngine;
using VContainer.Unity;

namespace Infrastructure.Data.Models.Static
{
    public class StaticDataModel : IStaticDataModel, IInitializable
    {
        private const string GameConfigPath = "StaticData/GameConfig";
        private const string GameBalancePath = "StaticData/GameBalance";

        public Config Config { get; private set; }
        public Balance Balance { get; private set; }

        public void Initialize() => Load();

        private void Load()
        {
            Config = Resources.Load<Config>(GameConfigPath);
            Balance = Resources.Load<Balance>(GameBalancePath);
        }
    }
}