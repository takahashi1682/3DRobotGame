using MyUtils;
using R3;
using UnityEngine;

namespace _Projects.Features.Unit
{
    /// <summary>
    /// 「実行中かどうか」を示すIsAction(ReactiveProperty)の宣言・公開を共通化する基底クラス。
    /// UnitMove/UnitFly/UnitBoost/UnitFire/UnitLockOn/PlayerFreeLookなど、
    /// IUnitActionHandler&lt;T&gt;を実装する各アクションクラスで同じ宣言が重複していたため抽出した。
    /// OnValueChanged/CancelActionの中身はアクションごとに異なるため、派生クラス側で実装する。
    /// </summary>
    public abstract class AbstractUnitAction : MonoBehaviour, IUnitActionObservable
    {
        [SerializeField, ReadOnly] protected SerializableReactiveProperty<bool> _isAction = new();
        public ReadOnlyReactiveProperty<bool> IsAction => _isAction;
    }
}
