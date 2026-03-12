using Entities.Components.Pathfinding;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.AI;

namespace Entities.Bakers.Pathfinding
{
    public class NavMeshModifierVolumeBaker : Baker<Unity.AI.Navigation.NavMeshModifierVolume>
    {
        public override void Bake(Unity.AI.Navigation.NavMeshModifierVolume authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new NavMeshModifierVolume
            {
                Size = authoring.size,
                Center = authoring.center,
                AreaType = authoring.area
            });

            DynamicBuffer<AffectedAgentElement> buffer = AddBuffer<AffectedAgentElement>(entity);

            int navMeshSettingsCount = NavMesh.GetSettingsCount();

            for (int i = 0; i < navMeshSettingsCount; i++)
            {
                NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByIndex(i);

                int agentID = buildSettings.agentTypeID;

                if (authoring.AffectsAgentType(agentID))
                    buffer.Add(new AffectedAgentElement { ID = agentID });
            }
        }
    }

    public struct NavMeshModifierVolume : IComponentData, IEnableableComponent
    {
        public float3 Size;
        public float3 Center;
        public int AreaType;
    }
}