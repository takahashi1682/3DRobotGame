using System.Collections.Generic;
using Features.Unit.Battle;
using UnityEngine;
using VContainer;

namespace Features.Unit
{
    public class UnitManager
    {
        private readonly List<UnitScopeRoot> _unitList = new();

        public void RegisterUnit(UnitScopeRoot unit)
        {
            _unitList.Add(unit);
        }

        public void RemoveUnit(UnitScopeRoot unit)
        {
            _unitList.Remove(unit);
        }

        /// <summary>
        /// currentから見て敵対する陣営のUnitのうち、maxDistance以内で最も近いものを返す。
        /// 該当がなければnull。
        /// </summary>
        public UnitScopeRoot FindClosestEnemyUnit(UnitScopeRoot current, float maxDistance)
        {
            var currentSetting = current.Container.Resolve<UnitSetting>();

            UnitScopeRoot closestEnemy = null;
            float closestDistance = maxDistance;

            foreach (var unit in _unitList)
            {
                if (!unit.Container.TryResolve(out UnitSetting targetSetting)) continue;
                if (!targetSetting.Army.IsTarget(currentSetting.Army)) continue;

                float distance = Vector3.Distance(
                    currentSetting.Pivot,
                    targetSetting.Pivot);
                if (distance <= closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = unit;
                }
            }

            return closestEnemy;
        }
    }
}