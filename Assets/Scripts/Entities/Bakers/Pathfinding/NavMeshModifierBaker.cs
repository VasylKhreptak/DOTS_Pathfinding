using Entities.Components.Pathfinding;
using Unity.Entities;
using UnityEngine.AI;

namespace Entities.Bakers.Pathfinding
{
    public class NavMeshModifierBaker : Baker<Unity.AI.Navigation.NavMeshModifier>
    {
        public override void Bake(Unity.AI.Navigation.NavMeshModifier authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            NavMeshModifier modifier = new NavMeshModifier
            {
                ApplyToChildren = authoring.applyToChildren,
                Mode = authoring.ignoreFromBuild ? NavMeshModifierMode.RemoveObject : NavMeshModifierMode.AddOrModifyObject,
                OverrideArea = authoring.overrideArea,
                Area = authoring.area,
                OverrideGenerateLinks = authoring.overrideGenerateLinks,
                GenerateLinks = authoring.generateLinks
            };

            AddComponent(entity, modifier);

            DynamicBuffer<AffectedAgentElement> affectedAgentsBuffer = AddBuffer<AffectedAgentElement>(entity);

            int navMeshSettingsCount = NavMesh.GetSettingsCount();

            for (int i = 0; i < navMeshSettingsCount; i++)
            {
                NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByIndex(i);

                int agentID = buildSettings.agentTypeID;

                if (authoring.AffectsAgentType(agentID))
                    affectedAgentsBuffer.Add(new AffectedAgentElement { ID = agentID });
            }
        }
    }

    public struct NavMeshModifier : IComponentData
    {
        public bool ApplyToChildren;
        public NavMeshModifierMode Mode;
        public bool OverrideArea;
        public int Area;
        public bool OverrideGenerateLinks;
        public bool GenerateLinks;
    }

    public enum NavMeshModifierMode
    {
        AddOrModifyObject,
        RemoveObject,
    }
}