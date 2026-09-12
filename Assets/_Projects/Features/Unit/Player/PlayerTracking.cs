using UnityEngine;

namespace _Projects.Features.Unit.Player
{
    public class PlayerTracking : UnitTracking
    {
        public Transform CameraTarget;

        protected override void Update()
        {
            base.Update();
            if (!IsTargetValid()) return;

            RotateCameraTowardsTarget();
        }

        /// <summary>
        /// カメラは上下方向(Pitch)のみ、現在の角度からターゲット方向へ一定速度で回転させる。
        /// Yaw(左右)は本体の回転に追従させるため、ここでは変更しない。
        /// </summary>
        private void RotateCameraTowardsTarget()
        {
            var diff = _targetPivot.position - CameraTarget.position;
            var horizontalDistance = new Vector2(diff.x, diff.z).magnitude;
            if (horizontalDistance < 0.0001f && Mathf.Abs(diff.y) < 0.0001f) return;

            // diffの成分をそのまま角度として使わず、Atan2で絶対角度(Pitch)を求める。
            var currentX = CameraTarget.eulerAngles.x;
            var targetX = -Mathf.Atan2(diff.y, horizontalDistance) * Mathf.Rad2Deg;
            var newX = Mathf.MoveTowardsAngle(currentX, targetX, _unitSetting.RotationSpeed * Time.deltaTime);

            var angle = CameraTarget.eulerAngles;
            angle.x = newX;
            CameraTarget.eulerAngles = angle;
        }
    }
}
