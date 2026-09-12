using System.Collections.Generic;
using _Projects.Features.Unit.Battle;
using R3;
using UnityEngine;

namespace _Projects.Features.Unit
{
    public class UnitManager
    {
        public List<UnitScopeRoot> UnitList { get; } = new();

        private readonly Subject<UnitScopeRoot> _onRegisteredUnitSubject = new();
        public Observable<UnitScopeRoot> OnRegisteredUnit => _onRegisteredUnitSubject;

        private readonly Subject<UnitScopeRoot> _onRemovedUnitSubject = new();
        public Observable<UnitScopeRoot> OnRemovedUnit => _onRemovedUnitSubject;

        public UnitManager(GameObject gameObject)
        {
            _onRegisteredUnitSubject.AddTo(gameObject);
            _onRemovedUnitSubject.AddTo(gameObject);
        }

        public void RegisterUnit(UnitScopeRoot unit)
        {
            UnitList.Add(unit);
            _onRegisteredUnitSubject.OnNext(unit);
        }

        public void RemoveUnit(UnitScopeRoot unit)
        {
            UnitList.Remove(unit);
            _onRemovedUnitSubject.OnNext(unit);
        }

        public List<UnitScopeRoot> GetTargetUnits(ArmyType current)
        {
            var units = new List<UnitScopeRoot>();

            foreach (var unit in UnitList)
            {
                if (unit.Setting == null) continue;
                if (!unit.Setting.Army.IsTarget(current)) continue;
                if (unit.Health == null) continue;
                if (unit.Health.IsEmpty.CurrentValue) continue;

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
                // targetUnitsはGetTargetUnitsで陣営・生存チェック済みなので、ここでは距離だけ見ればよい。
                float distance = Vector3.Distance(
                    currentPos,
                    unit.Setting.Pivot);
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