using System;
using MyUtils.VContainerExtensions;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Projects.Features.Unit.Player
{
    /// <summary>
    /// CinemachineはUpdate完了後(LateUpdate、または対象によってはFixedUpdate)にカメラの
    /// 実際のTransformを反映するため、それより後に実行しないと1フレーム古いカメラ位置を
    /// 参照してしまい、Dot/ロックオンUIがぶれる。DefaultExecutionOrderで他スクリプトの
    /// LateUpdate(CinemachineBrain含む)より後に実行されることを保証する。
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class PlayerLockOnViewer : MonoBehaviour, IUnitScopeMember, IScopeLaunchable
    {
        [SerializeField] private RectTransform _lockOnUI;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Slider _energySlider;
        [SerializeField] private RectTransform _dot;
        [SerializeField] private TMP_Text _distanceText;
        [Inject] private Camera _mainCamera;
        private UnitSetting _targetSetting;
        [Inject] private IUnitTrackingObservable _trackingObservable;
        private IDisposable _health;
        private IDisposable _energy;

        private void Awake()
        {
            _lockOnUI.gameObject.SetActive(false);
        }

        public void OnLaunch()
        {
            // Targetはロックオンの開始・解除・切り替えのタイミングでのみ変化を通知するので
            // (同じ値が連続で来ることはない)、ここで毎回購読を張り直せば十分。
            _trackingObservable.Target.Subscribe(target =>
            {
                _health?.Dispose();
                _energy?.Dispose();

                if (target != null)
                {
                    _targetSetting = target.Setting;

                    _health = target.Health.CurrentRate
                        .Subscribe(value => _healthSlider.value = value)
                        .AddTo(target);

                    _energy = target.Energy.CurrentRate
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

        private void LateUpdate()
        {
            _dot.position = _mainCamera.WorldToScreenPoint(_trackingObservable.TargetPosition);

            if (_trackingObservable?.Target.CurrentValue)
            {
                _lockOnUI.position = _mainCamera.WorldToScreenPoint(_targetSetting.Pivot);
                _distanceText.text = _trackingObservable.Distance.ToString("F1") + "m";
            }
        }
    }
}