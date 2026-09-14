using MyUtils.VContainerExtensions;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Game
{
    /// <summary>
    /// CinemachineのManualUpdateを実行するためのクラス。
    /// UpdatePhase.Cameraで各UnitがCameraTarget等を更新し終えた直後に、CinemachineBrain.ManualUpdateを
    /// 呼んで実際のカメラTransformを確定させる。これにより、後続のUI等が常にこのフレームの
    /// 最終的なカメラ位置を参照できる(LateUpdateのタイミングに依存しなくなる)。
    /// シーン側でCinemachineBrainのUpdateMethodをManualUpdateにしておくこと(自動更新との二重実行防止)。
    /// </summary>
    public class CinemachineManualUpdater : MonoBehaviour, IGameScopeMember, IScopeLaunchable, IPhaseUpdatable
    {
        [SerializeField] private CinemachineBrain _brain;
        [Inject] private UpdateDispatcher _dispatcher;

        public UpdatePhase Phase => UpdatePhase.Camera;

        public void OnLaunch()
        {
            _dispatcher.Register(this);
        }

        private void OnDestroy()
        {
            _dispatcher.Unregister(this);
        }

        public void OnPhaseUpdate()
        {
            _brain.ManualUpdate();
        }
    }
}