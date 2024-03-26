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

    /// <summary>
    /// Creates a new instance of the <see cref="NpcIdleState"/> class.
    /// In this state the Npc will idle on the map, moving around according to its movement settings.
    /// When attacked or when aggressive it will switch to <see cref="NpcCombatState"/> to attack valid targets.
    /// </summary>
    public NpcIdleState()
    {
    }

    public override void Update(long timeMs)
    {
        // If this NPC has recently moved, don't do anything.
        if (Npc.MoveTimer >= timeMs)
        {
            return;
        }

        // If our random movement timer has expired, move randomly.
        if (Npc.LastRandomMove < timeMs)
        {
            MoveRandomly(timeMs);

            // TODO: Do not hardcode movement timer?
            // Update our movement timer.
            Npc.LastRandomMove = timeMs + Randomization.Next(1000, 3000);
        }

        // Attempt to find a new target for glorious battle!
        Entity target = null;
        if (mLastTargetScan < timeMs && Npc.Base.Aggressive)
        {
            target = FindTarget();

            // TODO: Do not hardcode target scan timer?
            // Update our target scan timer.
            mLastTargetScan = timeMs + 500;
        }

        // Have we been attacked by something? If so, Retaliate!
        if (Npc.DamageMap.Count > 0)
        {
            target = Npc.DamageMap.ToArray().OrderByDescending(x => x.Value).FirstOrDefault().Key;
        }

        // It's time to d-d-d-d-duel!
        if (target != null)
        {
            StateMachine.SetState(new NpcCombatState(target));
        }

    }

    private void MoveRandomly(long timeMs)
    {
        //check if we are affected by a status effect that does not allow motion.
        foreach (var status in Npc.CachedStatuses)
        {
            if (status.Type == SpellEffect.Stun ||
                status.Type == SpellEffect.Snare ||
                status.Type == SpellEffect.Sleep)
            {
                return;
            }
        }

        // If our NPC movement type is standing still, simply randomize our movement timer again and exit out!
        if (Npc.Base.Movement == (int)NpcMovement.StandStill)
        {
            return;
        }
        // If our movement type is turning randomly, just turn and exit out!
        else if (Npc.Base.Movement == (int)NpcMovement.TurnRandomly)
        {
            Npc.ChangeDir(Randomization.NextDirection());
            return;
        }

        // Randomize whether we actually move or not.
        var i = Randomization.Next(0, 1);
        if (i == 0)
        {
            // Pick a random direction, check if we can move into it.
            var direction = Randomization.NextDirection();
            if (Npc.CanMoveInDirection(direction))
            {
                // Finally move!
                Npc.Move(direction, null);
            }
        }
    }

    private Entity FindTarget()
    {
        var possibleTargets = new List<Entity>();
        var closestRange = Npc.Range + 1;
        var closestIndex = -1;

        foreach (var instance in MapController.GetSurroundingMapInstances(Npc.MapId, Npc.MapInstanceId, true))
        {
            foreach (var entity in instance.GetCachedEntities())
            {
                if (entity != null && !entity.IsDead() && entity != Npc)
                {
                    //TODO Check if NPC is allowed to attack player with new conditions
                    if (entity is Player player)
                    {
                        if (Npc.ShouldAttackPlayerOnSight(player))
                        {
                            var dist = Npc.GetDistanceTo(entity);
                            if (dist <= Npc.Range && dist < closestRange)
                            {
                                possibleTargets.Add(entity);
                                closestIndex = possibleTargets.Count - 1;
                                closestRange = dist;
                            }
                        }
                    }
                    else if (entity is Entities.Npc npc)
                    {
                        if (Npc.Base.Aggressive && Npc.Base.AggroList.Contains(npc.Base.Id))
                        {
                            var dist = Npc.GetDistanceTo(entity);
                            if (dist <= Npc.Range && dist < closestRange)
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

}
