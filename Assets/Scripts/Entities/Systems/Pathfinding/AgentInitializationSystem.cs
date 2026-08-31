using Entities.Authoring.Pathfinding;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Entities.Systems.Pathfinding
{
    [BurstCompile]
    public partial struct AgentInitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state) => state.Dependency = new InitializeAgentsJob().ScheduleParallel(state.Dependency);

        [BurstCompile]
        [WithAll(typeof(Agent))]
        private partial struct InitializeAgentsJob : IJobEntity
        {
            public void Execute(in LocalTransform localTransform, ref Destination destination, EnabledRefRW<AgentNeedsInitializationFlag> agentNeedsInitializationFlag)
            {
                destination.Value = localTransform.Position;
                agentNeedsInitializationFlag.ValueRW = false;
            }
        }
    }
}