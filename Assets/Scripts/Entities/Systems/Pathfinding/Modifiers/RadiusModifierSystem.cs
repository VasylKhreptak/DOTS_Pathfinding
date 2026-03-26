using System;
using Entities.Authoring.Pathfinding;
using Entities.Authoring.Pathfinding.Modifiers;
using Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Entities.Systems.Pathfinding.Modifiers
{
    [BurstCompile]
    [DisableAutoCreation]
    public partial struct RadiusModifierSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TickCount>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            TickCount tickCount = SystemAPI.GetSingleton<TickCount>();

            RadiusModifierJob job = new RadiusModifierJob
            {
                TickCount = tickCount
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct RadiusModifierJob : IJobEntity
        {
            public TickCount TickCount;

            public void Execute(in PathFinder pathFinder, ref DynamicBuffer<PathWaypoint> path, in RadiusModifier modifier)
            {
                if (TickCount.Value != pathFinder.LastCalculationTickCount)
                    return;

                ApplyRadiusModifier(in pathFinder, ref path, in modifier);
            }

            private void ApplyRadiusModifier(in PathFinder pathFinder, ref DynamicBuffer<PathWaypoint> path, in RadiusModifier modifier)
            {
                if (path.Length < 3)
                    return;

                NativeArray<float> radi = new NativeArray<float>(path.Length, Allocator.Temp);
                NativeArray<float> a1 = new NativeArray<float>(path.Length, Allocator.Temp);
                NativeArray<float> a2 = new NativeArray<float>(path.Length, Allocator.Temp);
                NativeArray<bool> dir = new NativeArray<bool>(path.Length, Allocator.Temp);

                for (int i = 0; i < path.Length; i++)
                {
                    radi[i] = modifier.Radius;
                }

                radi[0] = 0;
                radi[path.Length - 1] = 0;

                int count = 0;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    count++;
                    if (count > 2 * path.Length)
                        break;

                    TangentType tt;

                    if (i == 0)
                        tt = CalculateTangentTypeSimple(path[i].Value, path[i + 1].Value, path[i + 2].Value);
                    else if (i == path.Length - 2)
                        tt = CalculateTangentTypeSimple(path[i - 1].Value, path[i].Value, path[i + 1].Value);
                    else
                        tt = CalculateTangentType(path[i - 1].Value, path[i].Value, path[i + 1].Value, path[i + 2].Value);

                    if ((tt & TangentType.Inner) != 0)
                    {
                        float a;
                        float sigma;

                        if (!CalculateCircleInner(path[i].Value, path[i + 1].Value, radi[i], radi[i + 1], out a, out sigma))
                        {
                            float magn = math.length(path[i + 1].Value - path[i].Value);
                            radi[i] = magn * (radi[i] / (radi[i] + radi[i + 1]));
                            radi[i + 1] = magn - radi[i];
                            radi[i] *= 0.99f;
                            radi[i + 1] *= 0.99f;
                            i -= 2;
                            continue;
                        }

                        if (tt == TangentType.InnerRightLeft)
                        {
                            a2[i] = sigma - a;
                            a1[i + 1] = sigma - a + (float)Math.PI;
                            dir[i] = true;
                        }
                        else
                        {
                            a2[i] = sigma + a;
                            a1[i + 1] = sigma + a + (float)Math.PI;
                            dir[i] = false;
                        }
                    }
                    else
                    {
                        float sigma;
                        float a;

                        if (!CalculateCircleOuter(path[i].Value, path[i + 1].Value, radi[i], radi[i + 1], out a, out sigma))
                        {
                            if (i == path.Length - 2)
                            {
                                radi[i] = math.length(path[i + 1].Value - path[i].Value);
                                radi[i] *= 0.99f;
                                i -= 1;
                            }
                            else
                            {
                                if (radi[i] > radi[i + 1])
                                    radi[i + 1] = radi[i] - math.length(path[i + 1].Value - path[i].Value);
                                else
                                    radi[i + 1] = radi[i] + math.length(path[i + 1].Value - path[i].Value);

                                radi[i + 1] *= 0.99f;
                            }

                            i -= 1;
                            continue;
                        }

                        if (tt == TangentType.OuterRight)
                        {
                            a2[i] = sigma - a;
                            a1[i + 1] = sigma - a;
                            dir[i] = true;
                        }
                        else
                        {
                            a2[i] = sigma + a;
                            a1[i + 1] = sigma + a;
                            dir[i] = false;
                        }
                    }
                }

                NativeList<float3> subdivided = new NativeList<float3>(Allocator.Temp);
                subdivided.Add(path[0].Value);

                float detail = modifier.Detail;

                if (detail < 1)
                    detail = 1;
                float step = 2 * math.PI / detail;
                for (int i = 1; i < path.Length - 1; i++)
                {
                    float start = a1[i];
                    float end = a2[i];
                    float rad = radi[i];

                    if (dir[i])
                    {
                        if (end < start)
                            end += math.PI * 2;
                        for (float t = start; t < end; t += step)
                        {
                            subdivided.Add(new float3(math.cos(t), 0, math.sin(t)) * rad + path[i].Value);
                        }
                    }
                    else
                    {
                        if (start < end)
                            start += (float)Math.PI * 2;
                        for (float t = start; t > end; t -= step)
                        {
                            subdivided.Add(new float3(math.cos(t), 0, math.sin(t)) * rad + path[i].Value);
                        }
                    }
                }

                subdivided.Add(path[path.Length - 1].Value);

                path.Clear();
                path.EnsureCapacity(subdivided.Length);
                for (int i = 0; i < subdivided.Length; i++)
                    path.Add(new PathWaypoint { Value = subdivided[i] });

                subdivided.Dispose();
                radi.Dispose();
                a1.Dispose();
                a2.Dispose();
                dir.Dispose();
            }

            private bool CalculateCircleInner(float3 p1, float3 p2, float r1, float r2, out float a, out float sigma)
            {
                float dist = math.length(p1 - p2);

                if (r1 + r2 > dist)
                {
                    a = 0;
                    sigma = 0;
                    return false;
                }

                a = math.acos((r1 + r2) / dist);

                sigma = math.atan2(p2.z - p1.z, p2.x - p1.x);
                return true;
            }

            private bool CalculateCircleOuter(float3 p1, float3 p2, float r1, float r2, out float a, out float sigma)
            {
                float dist = math.length(p1 - p2);

                if (math.abs(r1 - r2) > dist)
                {
                    a = 0;
                    sigma = 0;
                    return false;
                }

                a = math.acos((r1 - r2) / dist);
                sigma = math.atan2(p2.z - p1.z, p2.x - p1.x);
                return true;
            }

            private TangentType CalculateTangentType(float3 p1, float3 p2, float3 p3, float3 p4)
            {
                bool l1 = RightOrColinearXZ(p1, p2, p3);
                bool l2 = RightOrColinearXZ(p2, p3, p4);

                return (TangentType)(1 << ((l1 ? 2 : 0) + (l2 ? 1 : 0)));
            }

            private bool RightOrColinearXZ(float3 a, float3 b, float3 p) => (b.x - a.x) * (p.z - a.z) - (p.x - a.x) * (b.z - a.z) <= 0;

            private TangentType CalculateTangentTypeSimple(float3 p1, float3 p2, float3 p3)
            {
                bool l2 = RightOrColinearXZ(p1, p2, p3);
                bool l1 = l2;

                return (TangentType)(1 << ((l1 ? 2 : 0) + (l2 ? 1 : 0)));
            }
        }

        [Flags]
        private enum TangentType
        {
            OuterRight = 1 << 0,
            InnerRightLeft = 1 << 1,
            InnerLeftRight = 1 << 2,
            OuterLeft = 1 << 3,
            Outer = OuterRight | OuterLeft,
            Inner = InnerRightLeft | InnerLeftRight
        }
    }
}