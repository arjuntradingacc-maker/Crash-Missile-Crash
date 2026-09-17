using System;
using System.Collections.Generic;

namespace CrashMissileCrash.Data
{
    // JsonUtility cannot deserialize a root-level JSON array, so every content file's root
    // is an object with a single "items" array. These wrapper types exist purely for that.

    [Serializable] public class UnitDataCollection { public List<UnitData> items = new List<UnitData>(); }
    [Serializable] public class EnemyDataCollection { public List<EnemyData> items = new List<EnemyData>(); }
    [Serializable] public class BossDataCollection { public List<BossData> items = new List<BossData>(); }
    [Serializable] public class ChampionDataCollection { public List<ChampionData> items = new List<ChampionData>(); }
    [Serializable] public class CannonDataCollection { public List<CannonData> items = new List<CannonData>(); }
    [Serializable] public class CardDataCollection { public List<CardData> items = new List<CardData>(); }
    [Serializable] public class CardPackDataCollection { public List<CardPackData> items = new List<CardPackData>(); }
    [Serializable] public class GateDefinitionCollection { public List<GateDefinitionData> items = new List<GateDefinitionData>(); }
    [Serializable] public class WorldDataCollection { public List<WorldData> items = new List<WorldData>(); }
    [Serializable] public class LevelDataCollection { public List<LevelData> items = new List<LevelData>(); }
    [Serializable] public class MissionDataCollection { public List<MissionData> items = new List<MissionData>(); }
    [Serializable] public class LiveEventDataCollection { public List<LiveEventData> items = new List<LiveEventData>(); }
    [Serializable] public class SeasonDataCollection { public List<SeasonData> items = new List<SeasonData>(); }
    [Serializable] public class ShopOfferCollection { public List<ShopOfferData> items = new List<ShopOfferData>(); }
    [Serializable] public class LeagueDivisionCollection { public List<LeagueDivisionConfig> items = new List<LeagueDivisionConfig>(); }
}
