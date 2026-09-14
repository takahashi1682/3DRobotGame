using UnityEngine;

namespace _Projects.Features.Unit.Player
{
    public class PlayerTracking : UnitTracking
    {
        public Transform CameraTarget;

        public override void OnPhaseUpdate()
        {
            base.OnPhaseUpdate();
            if (!IsTargetValid()) return;

            RotateCameraTowardsTarget();
        }

        protected override Vector3 GetUnlockedTargetPosition()
        {
            return GetForwardPosition(CameraTarget);
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
            var targetX = -Mathf.Atan2(diff.y, horizontalDistance) * Mathf.Rad2Deg;

            var angle = CameraTarget.eulerAngles;
            angle.x = targetX;
            CameraTarget.eulerAngles = angle;
        }
    }
}