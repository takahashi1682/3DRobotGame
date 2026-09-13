using System.Collections.Generic;
using _Projects.Features.Unit.Battle;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

namespace _Projects.Features.Unit.Player
{
    /// <summary>
    /// 自機を中心としたレーダーUIに、生存中の各ユニットの位置を陣営色のアイコンで表示する。
    /// ユニット削除時、アイコンは破棄せずプールへ戻し、新規ユニット登録時に再利用する。
    /// </summary>
    public class PlayerRadarViewer : MonoBehaviour, IUnitScopeMember, IScopeLaunchable
    {
        [SerializeField] private float _radarScale = 2;
        [SerializeField] private RectTransform _radarUI;
        [SerializeField] private RectTransform _radarTargetIconPrefab;
        [FormerlySerializedAs("_arrayColor")] [SerializeField]
        private Color _allyColor;
        [SerializeField] private Color _enemyColor;
        [SerializeField] private Color _otherColor;

        private readonly Dictionary<UnitScopeRoot, RectTransform> _icons = new();
        private readonly Queue<RectTransform> _pooledIcons = new();

        [Inject] private UnitSetting _currentSetting;
        [Inject] private UnitManager _unitManager;

        public void OnLaunch()
        {
            // すでに存在するユニットのアイコンを生成
            foreach (var unit in _unitManager.UnitList)
            {
                CreateIcon(unit);
            }

            // 新しく生成されたユニットのアイコンを生成
            _unitManager.OnRegisteredUnit.Subscribe(CreateIcon).AddTo(this);

            // ユニットが削除されたときにアイコンをプールへ戻す
            _unitManager.OnRemovedUnit.Subscribe(ReleaseIcon).AddTo(this);
        }

        private void CreateIcon(UnitScopeRoot unit)
        {
            var icon = _pooledIcons.Count > 0
                ? _pooledIcons.Dequeue()
                : Instantiate(_radarTargetIconPrefab, _radarUI);
            icon.gameObject.SetActive(true);

            var image = icon.GetComponent<Image>();
            var army = unit.Setting.Army;
            image.color = GetArmyColor(army);

            _icons.Add(unit, icon);
        }

        private Color GetArmyColor(ArmyType army)
        {
            switch (army)
            {
                case ArmyType.PlayerAlly:
                    return _allyColor;

                case ArmyType.Enemy:
                case ArmyType.EnemyBoss:
                    return _enemyColor;

                default:
                    return _otherColor;
            }
        }

        /// <summary>
        /// アイコンを非表示にしてプールへ戻す。
        /// _iconsから即座に取り除くことで、破棄済みユニットをUpdateで毎フレーム参照し続けるのを防ぐ。
        /// </summary>
        private void ReleaseIcon(UnitScopeRoot unit)
        {
            if (!_icons.Remove(unit, out var icon)) return;

            icon.gameObject.SetActive(false);
            _pooledIcons.Enqueue(icon); // 使い終わったアイコンをプールへ戻す
        }

        private void Update()
        {
            foreach (var pair in _icons)
            {
                var setting = pair.Key.Setting;
                if (setting == null) continue;
                pair.Value.anchoredPosition = WorldToRadarPosition(setting.Pivot);
            }
        }

        /// <summary>
        /// ワールド座標を、自機を中心・自機の向きを基準にしたレーダーUI上の座標に変換する。
        /// </summary>
        private Vector3 WorldToRadarPosition(Vector3 worldPosition)
        {
            var diff = worldPosition - _currentSetting.Pivot;
            var radarPos = new Vector3(diff.x, diff.z, 0f) / _radarScale; // Y軸を無視してXZ平面に投影

            // 自機のY軸回転を考慮して、レーダー上の位置を回転させる
            float selfYaw = _currentSetting.UnitPivot.transform.eulerAngles.y;
            return Quaternion.Euler(0, 0, selfYaw) * radarPos;
        }
    }
}