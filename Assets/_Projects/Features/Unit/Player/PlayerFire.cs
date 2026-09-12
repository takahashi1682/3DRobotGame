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

        public override void OnResolve(IObjectResolver resolver)
        {
            base.OnResolve(resolver);
            var camera = resolver.Resolve<Camera>();
            var trackingObservable = resolver.Resolve<IUnitTrackingObservable>();

            // ロックオン対象がいない場合、カメラの正面方向を向くようにする。
            trackingObservable.Target.Subscribe(target =>
            {
                if (target != null) return;
                var lookPos = camera.transform.position + camera.transform.forward * BestFireDistance;
                Setting.FirePoint.LookAt(lookPos);
            }).AddTo(this);
        }
    }
}
