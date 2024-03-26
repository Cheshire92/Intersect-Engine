using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intersect.Server.Maps;

namespace Intersect.Server.Entities.States.Npc;
public abstract class NpcState : IEntityState
{
    public Entity Entity { get; set; }

    public EntityStateMachine StateMachine { get; set; }

    /// <summary>
    /// The <see cref="Entities.Npc"/> for which this state is executed.
    /// </summary>
    public Entities.Npc Npc { get; set; }

    /// <summary>
    /// Initializes the Entity State.
    /// Handle any setup that your state may require before updates start being called here.
    /// Don't forget to call base.Init() before anything else to set up your <see cref="Npc"/> reference!
    /// </summary>
    public virtual void Init()
    {
        Npc = (Entities.Npc)Entity;
    }

    public abstract void Update(long timeMs);

    public virtual void Delete()
    {
        StateMachine = null;
        Entity = null;
        Npc = null;
    }

}
