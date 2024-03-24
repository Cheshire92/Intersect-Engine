using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intersect.Server.Maps;

namespace Intersect.Server.Entities.States.Npc;
public abstract class NpcState : EntityState
{
    internal Entities.Npc mNpc;

    internal Guid mLastMap;

    public override void Init(Entity entity, EntityStateMachine entityStateMachine)
    {
        base.Init(entity, entityStateMachine);
        mNpc = (Entities.Npc)entity;
    }

    public override void Update(long timeMs)
    {
    }

}
