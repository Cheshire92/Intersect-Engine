using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intersect.Enums;
using Intersect.Server.Maps;
using Intersect.Utilities;

namespace Intersect.Server.Entities.States.Npc;
public class NpcIdleState : NpcState
{
    private long mLastTargetScan;

    private Entity mTarget;

    public override void Init(Entity entity, EntityStateMachine entityStateMachine)
    {
        base.Init(entity, entityStateMachine);
    }

    public override void Update(long timeMs)
    {
        // If this NPC has recently moved, don't do anything.
        if (mNpc.MoveTimer >= timeMs)
        {
            return;
        }

        // If our random movement timer has expired, move randomly.
        if (mNpc.LastRandomMove < timeMs)
        {
            MoveRandomly(timeMs);

            // TODO: Do not hardcode movement timer?
            // Update our movement timer.
            mNpc.LastRandomMove = timeMs + Randomization.Next(1000, 3000);
        }

        // Attempt to find a new target for glorious battle!
        if (mLastTargetScan < timeMs && mNpc.Base.Aggressive)
        {
            mTarget = FindTarget();

            // TODO: Do not hardcode target scan timer?
            // Update our target scan timer.
            mLastTargetScan = timeMs + 500;
        }

        // Have we been attacked by something? If so, Retaliate!
        if (mNpc.DamageMap.Count > 0)
        {
            mTarget = mNpc.DamageMap.ToArray().OrderByDescending(x => x.Value).FirstOrDefault().Key;
        }

        // It's time to d-d-d-d-duel!
        if (mTarget != null)
        {
            mEntityStateMachine.SetState(new NpcCombatState(mTarget));
        }

        // Update our base class at the end.
        base.Update(timeMs);
    }

    private void MoveRandomly(long timeMs)
    {
        // If our NPC movement type is standing still, simply randomize our movement timer again and exit out!
        if (mNpc.Base.Movement == (int)NpcMovement.StandStill)
        {
            return;
        }
        // If our movement type is turning randomly, just turn and exit out!
        else if (mNpc.Base.Movement == (int)NpcMovement.TurnRandomly)
        {
            mNpc.ChangeDir(Randomization.NextDirection());
            return;
        }

        // Randomize whether we actually move or not.
        var i = Randomization.Next(0, 1);
        if (i == 0)
        {
            // Pick a random direction, check if we can move into it.
            var direction = Randomization.NextDirection();
            if (mNpc.CanMoveInDirection(direction))
            {
                //check if we are affected by a status effect that does not allow motion.
                foreach (var status in mNpc.CachedStatuses)
                {
                    if (status.Type == SpellEffect.Stun ||
                        status.Type == SpellEffect.Snare ||
                        status.Type == SpellEffect.Sleep)
                    {
                        return;
                    }
                }

                // Finally move!
                mNpc.Move(direction, null);
            }
        }
    }

    private Entity FindTarget()
    {
        var possibleTargets = new List<Entity>();
        var closestRange = mNpc.Range + 1;
        var closestIndex = -1;

        foreach (var instance in MapController.GetSurroundingMapInstances(mNpc.MapId, mNpc.MapInstanceId, true))
        {
            foreach (var entity in instance.GetCachedEntities())
            {
                if (entity != null && !entity.IsDead() && entity != mNpc)
                {
                    //TODO Check if NPC is allowed to attack player with new conditions
                    if (entity is Player player)
                    {
                        if (mNpc.ShouldAttackPlayerOnSight(player))
                        {
                            var dist = mNpc.GetDistanceTo(entity);
                            if (dist <= mNpc.Range && dist < closestRange)
                            {
                                possibleTargets.Add(entity);
                                closestIndex = possibleTargets.Count - 1;
                                closestRange = dist;
                            }
                        }
                    }
                    else if (entity is Entities.Npc npc)
                    {
                        if (mNpc.Base.Aggressive && mNpc.Base.AggroList.Contains(npc.Base.Id))
                        {
                            var dist = mNpc.GetDistanceTo(entity);
                            if (dist <= mNpc.Range && dist < closestRange)
                            {
                                possibleTargets.Add(entity);
                                closestIndex = possibleTargets.Count - 1;
                                closestRange = dist;
                            }
                        }
                    }
                }
            }
        }

        if (closestIndex != -1)
        {
            return possibleTargets[closestIndex];
        }

        return null;
    }

    public override void Delete()
    {
        mNpc = null;
        mTarget = null;
    }

}
