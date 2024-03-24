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

    public EntityState LastState { get; private set; }

    public EntityState CurrentState { get; private set; }

    public EntityStateMachine(Entity entity, EntityState startingState)
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

    public void SetState(EntityState state)
    {
        // Dispose our last state
        LastState?.Dispose();

        // Tell our current state to finish whatever it is doing and set it to our last state for reference.
        CurrentState?.Delete();
        LastState = CurrentState;

        // Set a new current state and initialize it!
        CurrentState = state;
        CurrentState.Init(mEntity, this);
    }

    public void Dispose()
    {
        mEntity = null;
        CurrentState?.Dispose();
        CurrentState = null;
        LastState?.Dispose();
        LastState = null;
    }

}
