using System.Collections.Generic;
using Infrastructure.Services.Window.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "WindowFactoryConfig", menuName = "Configs/WindowFactoryConfig")]
    public class WindowFactoryConfig : SerializedScriptableObject
    {
        [Header("References")]
        [SerializeField] private GameObject _containerPrefab;
        [SerializeField] private GameObject _inputBlockerPrefab;
        [SerializeField] private Dictionary<WindowID, AssetReferenceGameObject> _windowsMap;

        public GameObject ContainerPrefab => _containerPrefab;
        public GameObject InputBlockerPrefab => _inputBlockerPrefab;
        public IReadOnlyDictionary<WindowID, AssetReferenceGameObject> WindowsMap => _windowsMap;
    }
}