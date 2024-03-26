using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Intersect.Server.Entities.States;
public class EntityStateMachine : IDisposable
{

    private Entity mEntity;

    public IEntityState LastState { get; private set; }

    public IEntityState CurrentState { get; private set; }

    public EntityStateMachine(Entity entity, IEntityState startingState)
    {
        mEntity = entity;
        SetState(startingState);
    }

    public void Update(long timeMs)
    {
        // Just in case the entity somehow ends up as deleted while processing this!
        if (mEntity == null)
        {
            return;
        }

        CurrentState?.Update(timeMs);
    }

    public void SetState(IEntityState state)
    {
        // Dispose our last state
        LastState?.Delete();

        // Tell our current state to finish whatever it is doing and set it to our last state for reference.
        CurrentState?.Delete();
        LastState = CurrentState;

        // Set a new current state and initialize it!
        CurrentState = state;
        CurrentState.Entity = mEntity;
        CurrentState.StateMachine = this;
        CurrentState.Init();
    }

    public void Dispose()
    {
        mEntity = null;
        CurrentState?.Delete();
        CurrentState = null;
        LastState?.Delete();
        LastState = null;
    }

}
