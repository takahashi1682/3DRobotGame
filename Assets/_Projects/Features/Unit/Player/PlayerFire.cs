using R3;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Unit.Player
{
    /// <summary>
    /// ロックオン対象がいない間、FirePointがメインカメラの正面方向(BestFireDistance先)を
    /// 向くようにする。ロックオン中の照準はUnitTracking側が担当する。
    /// </summary>
    public class PlayerFire : UnitFire
    {
        public float BestFireDistance = 100f;

        private Camera _camera;
        private IUnitTrackingObservable _trackingObservable;

        public override void OnResolve(IObjectResolver resolver)
        {
            base.OnResolve(resolver);
            _camera = resolver.Resolve<Camera>();
            _trackingObservable = resolver.Resolve<IUnitTrackingObservable>();
        }

        public override void OnStart()
        {
            base.OnStart();

            // ロックオン対象がいない場合、カメラの正面方向を向くようにする。
            _trackingObservable.Target.Subscribe(target =>
            {
                if (target != null) return;
                var lookPos = _camera.transform.position + _camera.transform.forward * BestFireDistance;
                Setting.FirePoint.LookAt(lookPos);
            }).AddTo(this);
        }
    }
}
