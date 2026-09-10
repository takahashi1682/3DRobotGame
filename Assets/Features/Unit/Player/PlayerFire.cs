using UnityEngine;
using VContainer;
using R3;

namespace Features.Unit.Player
{
    public class PlayerFire : UnitFire
    {
        [SerializeField] private Transform _camera;
        public float BestFireDistance = 100f;

        public override void OnResolve(IObjectResolver resolver)
        {
            base.OnResolve(resolver);
            var unitLockOn = resolver.Resolve<UnitLockOn>();
            var setting = resolver.Resolve<UnitSetting>();

            // ロックオン対象がいない場合、カメラの正面方向を向くようにする。
            unitLockOn.Target.Subscribe(target =>
            {
                if (target == null)
                {
                    var lookPos = _camera.position +
                                  _camera.forward * BestFireDistance;
                    setting.FirePoint.LookAt(lookPos);
                }
            }).AddTo(this);
        }
    }
}