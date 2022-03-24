namespace GMServer.Common
{
    public enum BonusType
    {
        NONE = 0,

        // Critical Chance
        FLAT_CRIT_CHANCE = 300,

        // Critical Damage
        MULTIPLY_CRIT_DMG = 400,

        // Prestige
        MULTIPLY_PRESTIGE_BONUS = 500,

        // Damage
        MULTIPLY_ALL_DMG = 600,
        MULTIPLY_MERC_DMG = 601,
        MULTIPLY_MELEE_DMG = 602,
        MULTIPLY_RANGED_DMG = 603,

        // Tap Damage
        MULTIPLY_TAP_DMG = 700,
        FLAT_TAP_DMG = 701,

        // Gold
        MULTIPLY_ALL_GOLD = 800,
        MULTIPLY_BOSS_GOLD = 801,
        MULTIPLY_ENEMY_GOLD = 802,
    }
}
