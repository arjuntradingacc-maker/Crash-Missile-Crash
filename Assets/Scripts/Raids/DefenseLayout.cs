using System.Collections.Generic;
using CrashMissileCrash.Base;

namespace CrashMissileCrash.Raids
{
    /// <summary>Snapshot of a player's base used to compute asynchronous raid outcomes.</summary>
    public class DefenseLayout
    {
        public Dictionary<BuildingType, int> BuildingLevels = new Dictionary<BuildingType, int>();

        public int TotalDefensePower()
        {
            int total = 0;
            foreach (var kv in BuildingLevels)
            {
                int weight = kv.Key == BuildingType.DefenseTower || kv.Key == BuildingType.ShieldGenerator ? 3 : 1;
                total += kv.Value * weight;
            }
            return total;
        }
    }
}
