using MyUtils;
using R3;
using UnityEngine;

namespace _Projects.Features.Unit
{
    /// <summary>
    /// IUnitControllableのうち、Move/Fire/Fly/Boost/LockOnの宣言・公開を共通化する基底クラス。
    /// PlayerInputReader(実入力)とAIControl(AI生成)で同じ宣言が重複していたため抽出した。
    /// LookDeviceNameは実装ごとに意味が異なる(実デバイス名 or 固定値)ため、派生クラス側で実装する。
    /// </summary>
    public abstract class AbstractUnitControllable : MonoBehaviour, IUnitControllable
    {
        [SerializeField, ReadOnly] protected SerializableReactiveProperty<Vector2> _move = new();
        public Observable<Vector2> Move => _move;

        [SerializeField, ReadOnly] protected SerializableReactiveProperty<Vector2> _look = new();
        public Observable<Vector2> Look => _look;

        public abstract string LookDeviceName { get; }

        [SerializeField, ReadOnly] protected SerializableReactiveProperty<bool> _fire = new();
        public Observable<bool> Fire => _fire;

        [SerializeField, ReadOnly] protected SerializableReactiveProperty<bool> _fly = new();
        public Observable<bool> Fly => _fly;

        [SerializeField, ReadOnly] protected SerializableReactiveProperty<bool> _boost = new();
        public Observable<bool> Boost => _boost;

        [SerializeField, ReadOnly] protected SerializableReactiveProperty<bool> _lockOn = new();
        public Observable<bool> LockOn => _lockOn;
    }
}