using UnityEngine;

namespace Entities.Authoring.Pathfinding.Movers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AgentAuthoring))]
    public abstract class PathMoverAuthoring : MonoBehaviour { }
}