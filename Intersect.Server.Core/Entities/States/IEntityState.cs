using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intersect.Server.Entities.States;

public interface IEntityState
{
    /// <summary>
    /// The <see cref="Entities.Entity"/> for which this state is executed.
    /// </summary>
    public Entity Entity { get; set; }

    /// <summary>
    /// The <see cref="EntityStateMachine"/> from which this state is executed.
    /// </summary>
    public EntityStateMachine StateMachine { get; set; }

    /// <summary>
    /// Initializes the Entity State.
    /// Handle any setup that your state may require before updates start being called here.
    /// </summary>
    public void Init();

    /// <summary>
    /// Updates the Entity State.
    /// Is called regularly upon updating the <see cref="Entities.Entity"/> that this is tied to.
    /// </summary>
    /// <param name="timeMs">The time at which the update method was called.</param>
    public void Update(long timeMs);

    /// <summary>
    /// Clear all of the Entity State's values and handle any last actions before removal of the state.
    /// </summary>
    public void Delete();
}
