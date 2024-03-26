using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intersect.Enums;
using Intersect.Server.Entities.Pathfinding;
using Intersect.Server.Maps;

namespace Intersect.Server.Entities.States.Npc;
public class NpcCombatState : NpcState
{
    private Entity mInitialTarget;

    private Entity mTarget => mNpc.Target;

    private Pathfinder mPathFinder => mNpc.PathFinder;

    private MapController mLeashMap;

    private int mLeashX;

    private int mLeashY;

    private int mLeashZ;

    /// <summary>
    /// Creates a new instance of the <see cref="NpcCombatState"/> class.
    /// In this state the Npc will attempt to fight other entities.
    /// Once it detects it is no longer allowed to attack due to the server configuration, it will change to <see cref="NPCResetState"/>.
    /// </summary>
    /// <param name="target">The <see cref="Entity"/> to attack initially.</param>
    /// <param name="oldLeashMap">OPTIONAL: The <see cref="MapController"/> to return to if a reset state is triggered.</param>
    /// <param name="oldLeashX">OPTIONAL: The X position to return to if a reset state is triggered.</param>
    /// <param name="oldLeashY">OPTIONAL: The Y position to return to if a reset state is triggered.</param>
    /// <param name="oldLeashZ">OPTIONAL: The Z position to return to if a reset state is triggered.</param>
    public NpcCombatState(Entity target, MapController oldLeashMap = null, int oldLeashX = -1, int oldLeashY = -1, int oldLeashZ = -1)
    {
        mInitialTarget = target;
        mLeashMap = oldLeashMap;
        mLeashX = oldLeashX;
        mLeashY = oldLeashY;
        mLeashZ = oldLeashZ;
    }

    public override void Init(Entity entity, EntityStateMachine entityStateMachine)
    {
        base.Init(entity, entityStateMachine);
        
        // Assign our target appropriately.
        mNpc.AssignTarget(mInitialTarget);

        // Set up our leash position, if not set by creating this instance.
        if (mLeashMap == null) 
        {
            mLeashMap = mNpc.Map;
            mLeashX = mNpc.X;
            mLeashY = mNpc.Y;
            mLeashZ = mNpc.Z;
        } 
    }

    public override void Update(long timeMs)
    {
        // If this NPC has recently moved, don't do anything.
        //if (mNpc.MoveTimer >= timeMs)
        //{
        //    return;
        //}

        // Try to cast a spell before attempting to do anything else!
        mNpc.TryCastSpells();
        
        // Update our movement towards our target.
        UpdateMovement(timeMs);

        // Try to atack our target!
        UpdateAttack(timeMs);

        // Should we reset based on our combat timer or leashing settings?
        if (ShouldExitCombat(timeMs))
        {
            mEntityStateMachine.SetState(new NPCResetState(mLeashMap, mLeashX, mLeashY, mLeashZ));
            return;
        }

        // Update our base class at the end.
        base.Update(timeMs);
    }

    private void UpdateMovement(long timeMs)
    {
        // Do we still have a target?
        if (mTarget != null)
        {
            // Don't bother with pathfinding logic for static NPCs.
            if (mNpc.Base.Movement != (int)NpcMovement.Static)
            {
                // Update our pathfinder target.
                UpdatePathFinderTarget();

                // Update our pathfinder movement.
                UpdatePathFinderMovement(timeMs);
            }
        }
    }

    private void UpdatePathFinderTarget()
    {
        // Check to see if the map of the target is in our current Mapgrid if the map is not the same.
        var targetMap = mTarget.MapId;
        var targetX = mTarget.X;
        var targetY = mTarget.Y;
        var targetZ = mTarget.Z;
        if (targetMap != mNpc.MapId)
        {
            var found = false;
            foreach (var map in MapController.Get(mNpc.MapId).SurroundingMaps)
            {
                if (map.Id == targetMap)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                targetMap = Guid.Empty;
            }
        }

        // Does the target map actually exist or is it within our grid? If so, update our pathfinder target.
        if (targetMap != Guid.Empty)
        {
            if (mPathFinder.GetTarget() != null)
            {
                if (targetMap != mPathFinder.GetTarget().TargetMapId ||
                    targetX != mPathFinder.GetTarget().TargetX ||
                    targetY != mPathFinder.GetTarget().TargetY)
                {
                    mPathFinder.SetTarget(null);
                }
            }
            if (mPathFinder.GetTarget() == null)
            {
                mPathFinder.SetTarget(new PathfinderTarget(targetMap, targetX, targetY, targetZ));
            }
        }
    }

    private void UpdatePathFinderMovement(long timeMs)
    {
        //check if NPC is snared or stunned
        foreach (var status in mNpc.CachedStatuses)
        {
            if (status.Type == SpellEffect.Stun ||
                status.Type == SpellEffect.Snare ||
                status.Type == SpellEffect.Sleep)
            {
                return;
            }
        }

        // Are we at our target yet?
        var pathtarget = mPathFinder.GetTarget();
        if (pathtarget != null && !IsOneBlockAway(pathtarget))
        {
            // We are not! CHARGE!
            switch (mPathFinder.Update(timeMs))
            {
                case PathfinderResult.Success:
                    var dir = mPathFinder.GetMove();
                    if (dir > Direction.None)
                    {
                        if (mNpc.CanMoveInDirection(dir, out var blockerType, out _) || blockerType == MovementBlockerType.Slide)
                        {
                            mNpc.Move(dir, null);
                        }
                        else
                        {
                            mPathFinder.PathFailed(timeMs);
                        }
                    }
                    break;
                case PathfinderResult.OutOfRange:
                case PathfinderResult.NoPathToTarget:
                case PathfinderResult.Failure:
                    mNpc.TryFindNewTarget(timeMs, mTarget?.Id ?? Guid.Empty, true);
                    break;
                case PathfinderResult.Wait:
                    // Tick, Tock
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private bool IsOneBlockAway(PathfinderTarget target) => mNpc.IsOneBlockAway(target.TargetMapId, target.TargetX, target.TargetY, target.TargetZ);

    private void UpdateAttack(long timeMs)
    {
        var pathtarget = mPathFinder.GetTarget();
        if (pathtarget != null && !IsOneBlockAway(pathtarget))
        {
            return;
        }

        if (mNpc.Dir != mNpc.DirectionToTarget(mTarget) && mNpc.DirectionToTarget(mTarget) != Direction.None)
        {
            mNpc.ChangeDir(mNpc.DirectionToTarget(mTarget));
        }
        else
        {
            if (mTarget == null)
            {
                mNpc.TryFindNewTarget(timeMs);
            }
            else
            {
                if (mNpc.CanAttack(mTarget, null))
                {
                    mNpc.TryAttack(mTarget);
                }
            }
        }
    }

    private bool ShouldExitCombat(long timeMs)
    {
        // Check whether our configuration allows us to check for a reset radius, if so check whether we've moved out of our boundaries.
        if (Options.Npc.AllowResetRadius && mLeashMap != null && (mNpc.GetDistanceTo(mLeashMap, mLeashX, mLeashY) > Math.Max(Options.Npc.ResetRadius, Math.Min(mNpc.Base.ResetRadius, Math.Max(Options.MapWidth, Options.MapHeight)))))
        {
            return true;
        }

        // Check whether our configuration allows us to reset after we've been out of combat for a specified amount of time.
        if (Options.Instance.NpcOpts.ResetIfCombatTimerExceeded && timeMs > mNpc.CombatTimer)
        {
            return true;
        }

        return false;
    }

    public override void Delete()
    {
        mInitialTarget = null;
        mLeashMap = null;
        mLeashX = 0;
        mLeashY = 0;
        mLeashZ = 0;
    }

}
