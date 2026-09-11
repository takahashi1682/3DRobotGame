using MyUtils.Parameter.Basic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using R3;
using TMPro;

namespace Features.Unit.Player
{
    public class PlayerLockOnViewer : MonoBehaviour, IUnitScopeInitializable
    {
        [SerializeField] private RectTransform _lockOnUI;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Slider _energySlider;
        [SerializeField] private TMP_Text _distanceText;
        private Camera _mainCamera;
        private UnitSetting _targetSetting;
        private IUnitTrackingObservable _unitTrackingObservable;


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
            _unitTrackingObservable = resolver.Resolve<IUnitTrackingObservable>();
            _unitTrackingObservable.Target.Subscribe(target =>
            {
                if (target != null)
                {
                    _targetSetting = target.Container.Resolve<UnitSetting>();

                    var targetHealth = target.Container.Resolve<Health>();
                    targetHealth.CurrentRate
                        .Subscribe(value => _healthSlider.value = value)
                        .AddTo(targetHealth);

                    var targetEnergy = target.Container.Resolve<Energy>();
                    targetEnergy.CurrentRate
                        .Subscribe(value => _energySlider.value = value)
                        .AddTo(targetEnergy);

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
            if (_unitTrackingObservable.Target.CurrentValue)
            {
                Vector3 screenPos = _mainCamera.WorldToScreenPoint(_targetSetting.Pivot);
                _lockOnUI.position = screenPos;

                _distanceText.text = _unitTrackingObservable.Distance.ToString("F1") + "m";
            }
        }
    }
}