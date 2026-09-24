using _Projects.Features.Unit.Battle;
using UnityEngine;

namespace _Projects.Features.Unit
{
    [System.Serializable]
    public class UnitSetting
    {
        [Header("Stats")]
        [field: SerializeField] public int MaxHealth { get; private set; } = 5000;
        [field: SerializeField] public int MaxEnergy { get; private set; } = 1000;

        [Header("References")]
        [field: SerializeField] public Transform UnitPivot { get; private set; }
        [field: SerializeField] public Transform FirePoint { get; private set; }

        [Header("Combat")]
        [field: SerializeField] public float RotationSpeed { get; private set; } = 400f;
        [field: SerializeField] public float MaxLockOnDistance { get; private set; } = 500f;
        [field: SerializeField] public ArmyType Army { get; private set; } = ArmyType.Player;

        [Tooltip("Targetとの間を遮る障害物とみなすレイヤー(視認判定に使用)")]
        [field: SerializeField] public LayerMask ObstacleLayerMask { get; private set; }

        /// <summary>
        /// 距離判定・ロックオンなどで使う、このUnitの基準座標。
        /// </summary>
        public Vector3 Pivot => UnitPivot.position;
    }
}