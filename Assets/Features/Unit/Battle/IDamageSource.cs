using VContainer;

namespace Features.Unit.Battle
{
    public interface IDamageSource
    {
        IObjectResolver Owner { get; } // 攻撃した本人
        int Damage { get; set; }
    }
}