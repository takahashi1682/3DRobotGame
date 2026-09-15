using MyUtils.VContainerExtensions;
using R3;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Game
{
    /// <summary>
    /// CinemachineBrainを手動更新する。CameraApplyフェーズ(各UnitがCameraTargetを動かした直後)で
    /// 呼ぶことで、後のUI処理が常に最新のカメラ位置を使えるようにする。
    /// ※CinemachineBrainのUpdateMethodはManualUpdateに設定しておくこと。
    /// </summary>
    public class CinemachineManualUpdater : MonoBehaviour, IGameScopeMember, IScopeLaunchable
    {
        [SerializeField] private CinemachineBrain _brain;
        [Inject] private IUpdateObservable _updateObservable;

        public void OnLaunch() =>
            _updateObservable.OnUpdate(EUpdatePhase.CameraApply)
                .Subscribe(_ => OnPhaseUpdate())
                .AddTo(this);

        public void OnPhaseUpdate()
        {
            _brain.ManualUpdate();
        }
    }
}