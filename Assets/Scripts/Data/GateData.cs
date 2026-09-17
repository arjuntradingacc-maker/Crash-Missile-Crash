using System;
using System.Collections.Generic;

namespace CrashMissileCrash.Data
{
    public enum GateOperation
    {
        Multiply,
        Add,
        Subtract,
        Divide,
        SetValue,
        RandomRange   // picks a random result between randomMin and randomMax units
    }

    public enum GateMotionType
    {
        Stationary,
        Sliding,      // moves back and forth horizontally
        Rotating,     // rotates around its own axis (visual + hit-window gameplay)
        Timed         // alternates open/closed on a timer
    }

    [Serializable]
    public class GatePlacementData
    {
        public string gateDefId;      // references a GateDefinitionData by id
        /// <summary>Normalized horizontal position within the lane, -1 (left) to 1 (right).</summary>
        public float laneX;
        public float distanceFromStart = 10f;
        /// <summary>If set, this gate is one of a split-path group; only one side should be taken.</summary>
        public string splitGroupId;
        public bool oneWay;
    }

    [Serializable]
    public class GateDefinitionData
    {
        public string id;
        public GateOperation operation = GateOperation.Multiply;
        public float value = 2f;
        public int randomMin = 1;
        public int randomMax = 1;
        public GateMotionType motion = GateMotionType.Stationary;
        public float motionRange = 2f;      // meters for sliding
        public float motionSpeed = 1f;      // units/sec for sliding/rotating
        public float openDuration = 2f;     // for Timed
        public float closedDuration = 1.5f; // for Timed
        public string label;                // display text override, e.g. "x2", "+25"
    }
}
