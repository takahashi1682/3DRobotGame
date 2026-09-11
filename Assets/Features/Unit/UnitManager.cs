using System;
using System.Collections.Generic;
using Features.Unit.Battle;
using MyUtils.Parameter.Basic;
using R3;
using UnityEngine;
using VContainer;

namespace Features.Unit
{
    public class UnitManager
    {
        private readonly List<UnitScopeRoot> _unitList = new();
        private readonly Subject<List<UnitScopeRoot>> _changedUnitListSubject = new();
        public Observable<List<UnitScopeRoot>> ChangedUnitList => _changedUnitListSubject;

        public UnitManager(GameObject gameObject)
        {
            _changedUnitListSubject.AddTo(gameObject);
        }

        public void RegisterUnit(UnitScopeRoot unit)
        {
            _unitList.Add(unit);
            _changedUnitListSubject.OnNext(_unitList);
        }

        public void RemoveUnit(UnitScopeRoot unit)
        {
            _unitList.Remove(unit);
            _changedUnitListSubject.OnNext(_unitList);
        }

        public List<UnitScopeRoot> GetTargetUnits(ArmyType current)
        {
            var units = new List<UnitScopeRoot>();

            foreach (var unit in _unitList)
            {
                if (!unit.Container.TryResolve(out UnitSetting targetSetting)) continue;
                if (!targetSetting.Army.IsTarget(current)) continue;
                if (!unit.Container.TryResolve(out Health targetHealth)) continue;
                if (targetHealth.IsEmpty.CurrentValue) continue;

                units.Add(unit);
            }

            return units;
        }

        /// <summary>
        /// currentから見て敵対する陣営のUnitのうち、maxDistance以内で最も近いものを返す。
        /// 該当がなければnull。
        /// </summary>
        public UnitScopeRoot FindClosestTargetUnit(ArmyType current, Vector3 currentPos, float maxDistance)
        {
            var targetUnits = GetTargetUnits(current);

            UnitScopeRoot closestEnemy = null;
            float closestDistance = maxDistance;

            foreach (var unit in targetUnits)
            {
                if (!unit.Container.TryResolve(out UnitSetting targetSetting)) continue;
                if (!targetSetting.Army.IsTarget(current)) continue;
                if (!unit.Container.TryResolve(out Health targetHealth)) continue;
                if (targetHealth.IsEmpty.CurrentValue) continue;

                float distance = Vector3.Distance(
                    currentPos,
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