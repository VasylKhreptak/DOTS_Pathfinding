using Entities.Authoring.Pathfinding;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Editor.Pathfinding
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class PathDrawSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            foreach (DynamicBuffer<PathWaypoint> waypoints in SystemAPI.Query<DynamicBuffer<PathWaypoint>>())
            {
                if (waypoints.Length < 2)
                    continue;

                for (int i = 0; i < waypoints.Length - 1; i++)
                {
                    float3 a = waypoints[i].Value;
                    float3 b = waypoints[i + 1].Value;

                    Debug.DrawLine(a, b, Color.green);
                }
            }
        }
    }
}