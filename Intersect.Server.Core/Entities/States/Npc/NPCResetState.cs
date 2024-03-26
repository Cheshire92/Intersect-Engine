using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intersect.Enums;
using Intersect.Server.Maps;
using Intersect.Server.Networking;
using Intersect.Utilities;

namespace Intersect.Server.Entities.States.Npc;
public class NPCResetState : NpcState
{

    private MapController mDestMap;

    private int mDestX;

    private int mDestY;

    private int mDestZ;

    private int mLastDistance;

    private int mResetFailureCounter;

    /// <summary>
    /// Creates a new instance of the <see cref="NPCResetState"/> class.
    /// In this state the Npc will reset according to server settings, i.e. it can reset vitals and move to its last known idle location if configured to do so.
    /// If allowed by server settings, it will re-engage in combat and return to <see cref="NpcCombatState"/>.
    /// If the reset is completed, it will return to <see cref="NpcIdleState"/>/
    /// </summary>
    /// <param name="map">The <see cref="MapController"/> to return to if configured to do so.</param>
    /// <param name="x">The X position to return to if configured to do so.</param>
    /// <param name="y">The Y position to return to if configured to do so.</param>
    /// <param name="z">The Z position to return to if configured to do so.</param>
    public NPCResetState(MapController map, int x, int y, int z)
    {
        mDestMap = map;
        mDestX = x;
        mDestY = y;
        mDestZ = z;
    }

    public override void Init(Entity entity, EntityStateMachine entityStateMachine)
    {
        base.Init(entity, entityStateMachine);

        // Reset our Npc, update our vitals if configured to do so.
        mNpc.Reset(Options.Npc.ResetVitalsAndStatusses);
        
        // Send our aggression update to let people know our anger was just a phase.
        PacketSender.SendNpcAggressionToProximity(mNpc);
    }

    public override void Update(long timeMs)
    {
        // If we do not have the server configured to handle a reset radius, simply drop the NPC back into the Idle state!
        if (!Options.Npc.AllowResetRadius)
        {
            mEntityStateMachine.SetState(new NpcIdleState());
            return;
        }

        // If we have somehow obtained a target and we are allowed to return to battle, engage!
        if (Options.Npc.AllowEngagingWhileResetting && mNpc.Target != null && !mNpc.Target.IsDead() && mNpc.InRangeOf(mNpc.Target, Options.MapWidth * 2))
        {
            // If our configuration allows for a new reset location before we have reset, update it by not passing our old one along.
            if (Options.Npc.AllowNewResetLocationBeforeFinish)
            {
                mEntityStateMachine.SetState(new NpcCombatState(mNpc.Target));
            }
            // Otherwise, pass our old values along to keep the reset location intact.
            else
            {
                mEntityStateMachine.SetState(new NpcCombatState(mNpc.Target, mDestMap, mDestX, mDestY, mDestZ));
            }
            
            return;
        }

        // Reset our vitals on every update if configured to do so.
        mNpc.Reset(Options.Npc.ContinuouslyResetVitalsAndStatuses);

        // ******************************************************
        // TODO: Move you lazy bugger!
        // ******************************************************

        // Check if we've arrived at our reset destination, if so return to idle.
        var distance = mNpc.GetDistanceTo(mDestMap, mDestZ, mDestY);
        if (distance < 1) 
        {
            mEntityStateMachine.SetState(new NpcIdleState());
            return;
        }
        // Okay, so we have not reached our destination yet.
        else
        {
            // Has our position changed at all?
            if (distance != mLastDistance)
            {
                // Yes, so update the value accordingly.
                mLastDistance = distance;
            }
            // No, that's odd?
            else
            {
                // Something fishy is going on here, we can't reset for whatever reason. Add to a counter.
                mResetFailureCounter++;

                // If we somehow have not managed to move after 100 updates, return to idle as this isn't right!
                // TODO: Make the reset counter configurable?
                if (mResetFailureCounter >= 100)
                {
                    mEntityStateMachine.SetState(new NpcIdleState());
                    return;
                }
            }

            // ******************************************************
            // TODO: Find a new target to harass if allowed, you freak!
            // ******************************************************
        }

        // Update our base class at the end.
        base.Update(timeMs);
    }

    public override void Delete()
    {
    }

}
