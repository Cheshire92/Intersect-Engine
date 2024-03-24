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
        if (mNpc == null)
        {
            return;
        }

        //Have we switched maps somewhere along the lines? If so, remove ourselves from the old and add ourselves to the new!
        if (mLastMap != mNpc.MapId)
        {
            if (mLastMap == Guid.Empty)
            {
                if (MapController.TryGetInstanceFromMap(mLastMap, mNpc.MapInstanceId, out var instance))
                {
                    instance.RemoveEntity(mNpc);
                }
            }
            if (mNpc.MapId != Guid.Empty)
            {
                if (MapController.TryGetInstanceFromMap(mLastMap, mNpc.MapInstanceId, out var instance))
                {
                    instance.AddEntity(mNpc);
                }
            }
        }

        mLastMap = mNpc.MapId;
    }

}
