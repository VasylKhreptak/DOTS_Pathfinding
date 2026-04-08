using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    [RequireComponent(typeof(DestinationAuthoring))]
    [RequireComponent(typeof(PathAuthoring))]
    [RequireComponent(typeof(SeekerAuthoring))]
    public class AgentAuthoring : MonoBehaviour
    {
        [SerializeField] private int _agentType;

        private class AgentBaker : Baker<AgentAuthoring>
        {
            public override void Bake(AgentAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                Agent agent = new Agent()
                {
                    AgentID = authoring._agentType
                };

                AddComponent(entity, agent);
            }
        }
    }

    public struct Agent : IComponentData
    {
        public int AgentID;
        public bool ReachedEndOfPath;
        public bool ReachedDestination;
    }
}