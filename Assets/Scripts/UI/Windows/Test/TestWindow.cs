using Cysharp.Threading.Tasks;
using Infrastructure.UI.Windows.Core;
using UnityEngine;

namespace UI.Windows.Test
{
    public class TestWindow : FadeNavigationalWindow
    {
        [Header("Preferences")]
        [SerializeField] private GameObject _firstSelectedGameObject;

        public override UniTask Show() => base.Show().ContinueWith(() => SelectGameObjectIfActive(_firstSelectedGameObject));
    }
}