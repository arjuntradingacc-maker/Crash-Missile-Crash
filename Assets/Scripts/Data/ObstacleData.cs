using System;

namespace CrashMissileCrash.Data
{
    public enum ObstacleType
    {
        SpeedZone,
        LaunchPad,
        MovingPlatform,
        RotatingObstacle,
        Crusher,
        EnemyCannon,
        DefensiveTower,
        Bridge,
        Teleporter,
        DestructibleBarrier,
        MovingWall
    }

    [Serializable]
    public class ObstaclePlacementData
    {
        public string id;
        public ObstacleType type = ObstacleType.SpeedZone;
        public float laneX;
        public float distanceFromStart = 10f;

        // Generic tunables - meaning depends on `type`; kept flat for JsonUtility compatibility.
        public float paramA = 1f;   // SpeedZone: speed multiplier | LaunchPad: launch force | Crusher: damage
        public float paramB = 1f;   // MovingPlatform/Wall: travel range | RotatingObstacle: rotation speed
        public float paramC = 1f;   // Timing: cycle duration
        public float health = 0f;   // DestructibleBarrier/DefensiveTower/EnemyCannon
        public string linkedTeleporterId; // Teleporter: id of paired exit
        public string visualKey;
    }
}
