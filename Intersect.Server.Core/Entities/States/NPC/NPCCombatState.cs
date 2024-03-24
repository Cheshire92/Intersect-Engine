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

    private bool mFleeing;

    public NpcCombatState(Entity target)
    {
        mInitialTarget = target;
    }

    public override void Init(Entity entity, EntityStateMachine entityStateMachine)
    {
        base.Init(entity, entityStateMachine);
        mNpc = (Entities.Npc)entity;
        mNpc.AssignTarget(mInitialTarget);
    }

    public override void Update(long timeMs)
    {
        // If this NPC has recently moved, don't do anything.
        if (mNpc.MoveTimer >= timeMs)
        {
            return;
        }

        // Update our movement towards our target.
        UpdateMovement(timeMs);

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
                        if (mFleeing)
                        {
                            switch (dir)
                            {
                                case Direction.Up:
                                    dir = Direction.Down;
                                    break;
                                case Direction.Down:
                                    dir = Direction.Up;
                                    break;
                                case Direction.Left:
                                    dir = Direction.Right;
                                    break;
                                case Direction.Right:
                                    dir = Direction.Left;
                                    break;
                                case Direction.UpLeft:
                                    dir = Direction.UpRight;
                                    break;
                                case Direction.UpRight:
                                    dir = Direction.UpLeft;
                                    break;
                                case Direction.DownRight:
                                    dir = Direction.DownLeft;
                                    break;
                                case Direction.DownLeft:
                                    dir = Direction.DownRight;
                                    break;
                            }
                        }
                        if (mNpc.CanMoveInDirection(dir, out var blockerType, out _) || blockerType == MovementBlockerType.Slide)
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
                            mNpc.Move(dir, null);
                        }
                        else
                        {
                            mPathFinder.PathFailed(timeMs);
                        }
                    }
                    break;
                case PathfinderResult.OutOfRange:
                    //TryFindNewTarget(timeMs, tempTarget?.Id ?? Guid.Empty, true);
                    //tempTarget = Target;
                    //targetMap = Guid.Empty;
                    break;
                case PathfinderResult.NoPathToTarget:
                    //TryFindNewTarget(timeMs, tempTarget?.Id ?? Guid.Empty, true);
                    //tempTarget = Target;
                    //targetMap = Guid.Empty;
                    break;
                case PathfinderResult.Failure:
                    //targetMap = Guid.Empty;
                    //TryFindNewTarget(timeMs, tempTarget?.Id ?? Guid.Empty, true);
                    //tempTarget = Target;
                    break;
                case PathfinderResult.Wait:
                    //targetMap = Guid.Empty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private bool IsOneBlockAway(PathfinderTarget target) => mNpc.IsOneBlockAway(target.TargetMapId, target.TargetX, target.TargetY, target.TargetZ);

    public override void Delete()
    {
    }

}
