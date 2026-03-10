using Udar.SceneManager;
using UnityEngine;
using LogType = Infrastructure.Services.Log.Core.LogType;

namespace Infrastructure.Data.Models.Static.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "ScriptableObjects/Static/GameConfig", order = 0)]
    public class Config : ScriptableObject
    {
        [Header("Scenes")]
        [SerializeField] private SceneField _bootstrapScene;
        [SerializeField] private SceneField _menuScene;
        [SerializeField] private SceneField _gameplayScene;

        [Header("Log Preferences")]
        [SerializeField] private LogType _editorLogType = LogType.Info;
        [SerializeField] private LogType _buildLogType = LogType.Info;

        public string BootstrapScene => _bootstrapScene.Name;
        public string MenuScene => _menuScene.Name;
        public string GameplayScene => _gameplayScene.Name;

        public LogType LogType => Application.isEditor ? _editorLogType : _buildLogType;
    }
}