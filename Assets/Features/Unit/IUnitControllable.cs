using R3;
using UnityEngine;

namespace Features.Unit
{
    public interface IUnitControllable
    {
        public Observable<Vector2> Move { get; }
        public Observable<Vector2> Look { get; }
        public string LookDeviceName { get; }
        public Observable<bool> Fly { get; }
        public Observable<bool> Boost { get; }
        public Observable<bool> Fire { get; }
        public Observable<bool> LockOn { get; }
    }
}