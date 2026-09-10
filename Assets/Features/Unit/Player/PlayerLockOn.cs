using UnityEngine;

namespace Features.Unit.Player
{
    public class PlayerLockOn : UnitLockOn
    {
        public Transform CameraTarget;

        protected override void LookAtTarget(Vector3 targetPos)
        {
            base.LookAtTarget(targetPos);

            // カメラの回転をターゲットに向ける
            if (TryGetLookRotation(targetPos, CameraTarget.position, out var cameraRotation))
            {
                CameraTarget.localRotation = Quaternion.Euler(cameraRotation.eulerAngles.x, 0, 0);
            }
        }
    }
}
