using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Projects.Features.Unit.Player
{
    public class PlayerLockOnViewer : MonoBehaviour, IUnitScopeInitializable
    {
        [SerializeField] private RectTransform _lockOnUI;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Slider _energySlider;
        [SerializeField] private TMP_Text _distanceText;
        private Camera _mainCamera;
        private UnitSetting _targetSetting;
        private IUnitTrackingObservable _trackingObservable;

        private void Start()
        {
            _lockOnUI.gameObject.SetActive(false);
        }

        public void OnRegister(IContainerBuilder builder)
        {
        }

        public void OnResolve(IObjectResolver resolver)
        {
            _mainCamera = resolver.Resolve<Camera>();
            _trackingObservable = resolver.Resolve<IUnitTrackingObservable>();

            IDisposable health = null;
            IDisposable energy = null;

            // Targetはロックオンの開始・解除・切り替えのタイミングでのみ変化を通知するので
            // (同じ値が連続で来ることはない)、ここで毎回購読を張り直せば十分。
            _trackingObservable.Target.Subscribe(target =>
            {
                health?.Dispose();
                energy?.Dispose();

                if (target != null)
                {
                    _targetSetting = target.Setting;

                    health = target.Health.CurrentRate
                        .Subscribe(value => _healthSlider.value = value)
                        .AddTo(target);

                    energy = target.Energy.CurrentRate
                        .Subscribe(value => _energySlider.value = value)
                        .AddTo(target);

                    _lockOnUI.gameObject.SetActive(true);
                }
                else
                {
                    _lockOnUI.gameObject.SetActive(false);
                }
            }).AddTo(this);
        }

        private void Update()
        {
            if (_trackingObservable.Target.CurrentValue)
            {
                Vector3 screenPos = _mainCamera.WorldToScreenPoint(_targetSetting.Pivot);
                _lockOnUI.position = screenPos;

                _distanceText.text = _trackingObservable.Distance.ToString("F1") + "m";
            }
        }
    }
}