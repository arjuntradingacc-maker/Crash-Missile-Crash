using System;

namespace CrashMissileCrash.Data
{
    public enum ProjectileType
    {
        SingleUnit,
        SplitShot,      // spawns 2 units diverging slightly
        PiercingShot,   // spawned unit has splash on landing
        HomingShot
    }

    [Serializable]
    public class CannonData
    {
        public string id;
        public string displayName;
        public Rarity rarity = Rarity.Common;
        public float fireRate = 4f;              // shots per second
        public ProjectileType projectileType = ProjectileType.SingleUnit;
        public string spawnUnitId;               // which UnitData a shot spawns
        public int unitsPerShot = 1;
        public float damageMultiplier = 1f;       // multiplies spawned unit stats
        public string specialEffectId;            // e.g. "muzzle_flare_gold"
        public string visualKey;
        public int maxLevel = 10;
        public float perLevelFireRateGrowth = 0.15f;
        public float perLevelDamageGrowth = 0.1f;
    }
}
