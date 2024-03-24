using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intersect.Server.Entities.States;

public abstract class EntityState : IDisposable
{
    private Entity mEntity;

    private EntityStateMachine mEntityStateMachine;

    public virtual void Init(Entity entity, EntityStateMachine entityStateMachine)
    {
        mEntity = entity;
        mEntityStateMachine = entityStateMachine;
    }

    public abstract void Update(long timeMs);

    public abstract void Delete();

    public void Dispose()
    {
        mEntity = null;
        mEntityStateMachine = null;
    }
}
