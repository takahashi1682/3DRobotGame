using MyUtils;
using R3;
using UnityEngine;

namespace _Projects.Features.Unit
{
    /// <summary>
    /// 「実行中かどうか」を表すIsActionをまとめた基底クラス。
    /// UnitMoveやUnitFireなど、各アクションクラスで共通する部分を1つにまとめている。
    /// </summary>
    public abstract class AbstractUnitAction : MonoBehaviour, IUnitActionObservable
    {
        [SerializeField, ReadOnly] protected SerializableReactiveProperty<bool> _isAction = new();
        public ReadOnlyReactiveProperty<bool> IsAction => _isAction;
    }
}
