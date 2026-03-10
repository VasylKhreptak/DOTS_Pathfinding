using Infrastructure.Services.Tickable.Core;
using Infrastructure.UI.Windows.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace Infrastructure.UI
{
    public class PointerOverAutoSelect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITickable
    {
        [SerializeField] private BaseNavigationalWindow _baseNavigationalWindow;

        private ITickableService _tickableService;

        [Inject]
        public void Construct(ITickableService tickableService)
        {
            _tickableService = tickableService;
        }

        #region MonoBehaviour

        private void OnValidate() => _baseNavigationalWindow ??= GetComponentInParent<BaseNavigationalWindow>();

        private void OnDisable() => _tickableService.Remove(this);

        #endregion

        public void Tick() => _baseNavigationalWindow.SelectGameObjectIfActive(gameObject);

        public void OnPointerEnter(PointerEventData eventData) => _tickableService.Add(this);

        public void OnPointerExit(PointerEventData eventData) => _tickableService.Remove(this);
    }
}