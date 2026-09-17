namespace CrashMissileCrash.Battle
{
    /// <summary>A stationary defensive structure guarding the enemy base. Reuses EnemyUnit's
    /// targeting/attack behaviour but never leaves its post.</summary>
    public class BaseTower : EnemyUnit
    {
        public override bool IsStructure => true;

        public override void TickMovement(float deltaTime)
        {
            // Stationary: intentionally does not move toward its target, only attacks in range.
        }
    }
}
