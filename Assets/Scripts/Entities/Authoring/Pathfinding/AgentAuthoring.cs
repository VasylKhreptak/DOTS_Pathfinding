using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    [RequireComponent(typeof(DestinationAuthoring))]
    [RequireComponent(typeof(PathAuthoring))]
    [RequireComponent(typeof(SeekerAuthoring))]
    public class AgentAuthoring : MonoBehaviour
    {
        private class AgentBaker : Baker<AgentAuthoring>
        {
            public override void Bake(AgentAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                Agent agent = new Agent();

                AddComponent(entity, agent);
            }
        }
    }

    public struct Agent : IComponentData, IEnableableComponent
    {
        public int AgentID;
    }
}