using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intersect.Server.Entities.States.NPC;
public class NPCCombatState : EntityState
{
    private Npc mNpc;

    private Entity mInitialTarget;

    public NPCCombatState(Entity target)
    {
        mInitialTarget = target;
    }

    public override void Init(Entity entity, EntityStateMachine entityStateMachine)
    {
        base.Init(entity, entityStateMachine);
        mNpc = (Npc)entity;
        mNpc.AssignTarget(mInitialTarget);
    }

    public override void Update(long timeMs)
    {
    }

    public override void Delete()
    {
    }

}
