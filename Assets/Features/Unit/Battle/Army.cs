namespace Features.Unit.Battle
{
    /// <summary>
    /// 列挙型定数：軍の種類
    /// </summary>
    public enum ArmyType
    {
        Player, // プレイヤー陣営
        PlayerAlly, // プレイヤーの味方陣営
        EnemyBoss, // ボス陣営
        Enemy, // 敵陣営
        Other, // その他の陣営（中立など）
        Gimmick, // ギミック陣営（トラップなど）
        None, // 陣営なし（敵対関係なし）
    }

    public static class ArmyExtensions
    {
        private static bool IsPlayerSide(this ArmyType type)
        {
            return type is ArmyType.Player or ArmyType.PlayerAlly;
        }

        private static bool IsEnemySide(this ArmyType type)
        {
            return type is ArmyType.Enemy or ArmyType.EnemyBoss;
        }

        /// <summary>
        /// ロックオンなど、索敵・照準の対象にできるかどうかの判定。
        /// Player系(Player/PlayerAlly)とEnemy系(Enemy/Boss)が対立する関係にある場合のみtrue。
        /// Other/Gimmick/Noneは索敵対象にならない。
        /// </summary>
        public static bool IsTarget(this ArmyType current, ArmyType target)
        {
            return current.IsPlayerSide() && target.IsEnemySide() ||
                   current.IsEnemySide() && target.IsPlayerSide();
        }

        /// <summary>
        /// ダメージが届くかどうかの判定。
        /// IsTargetの対立関係に加え、Gimmick(トラップなど)は陣営を問わずPlayer系・Enemy系の両方に当たる。
        /// </summary>
        public static bool CanHit(this ArmyType current, ArmyType target)
        {
            if (current == ArmyType.Gimmick)
            {
                return target.IsPlayerSide() || target.IsEnemySide();
            }

            return current.IsTarget(target);
        }
    }
}