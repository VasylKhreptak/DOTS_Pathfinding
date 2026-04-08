using System.Reflection;
using Entities.Authoring.Pathfinding;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Editor.Pathfinding
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class PathDrawSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (Selection.objects != null && Selection.objects.Length > 0)
            {
                foreach (Object obj in Selection.objects)
                {
                    if (obj.GetType().Name == "EntitySelectionProxy")
                    {
                        FieldInfo entityIndexField = obj.GetType().GetField("entityIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                        FieldInfo entityVersionField = obj.GetType().GetField("entityVersion", BindingFlags.NonPublic | BindingFlags.Instance);

                        if (entityIndexField != null && entityVersionField != null)
                        {
                            int entityIndex = (int)entityIndexField.GetValue(obj);
                            int entityVersion = (int)entityVersionField.GetValue(obj);

                            Entity entity = new Entity() { Index = entityIndex, Version = entityVersion };

                            if (EntityManager.HasBuffer<PathWaypoint>(entity))
                            {
                                DynamicBuffer<PathWaypoint> waypoints = EntityManager.GetBuffer<PathWaypoint>(entity);

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
            }
        }
    }
}