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
    public partial struct SmoothModifierSystem : ISystem
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

            SmoothModifierJob job = new SmoothModifierJob
            {
                TickCount = tickCount
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct SmoothModifierJob : IJobEntity
        {
            public TickCount TickCount;

            public void Execute(in Seeker seeker, ref DynamicBuffer<PathWaypoint> path, in SmoothModifier modifier)
            {
                if (TickCount.Value != seeker.LastUpdateTickCount)
                    return;

                switch (modifier.SmoothType)
                {
                    case SmoothType.Simple:
                        ApplySimpleModifier(in seeker, ref path, in modifier);
                        break;
                    case SmoothType.Bezier:
                        ApplyBezierModifier(in seeker, ref path, in modifier);
                        break;
                    case SmoothType.OffsetSimple:
                        ApplyOffsetModifier(in seeker, ref path, in modifier);
                        break;
                    case SmoothType.CurvedNonuniform:
                        ApplyCurvedNonuniformModifier(in seeker, ref path, in modifier);
                        break;
                }
            }

            private void ApplySimpleModifier(in Seeker seeker, ref DynamicBuffer<PathWaypoint> path, in SmoothModifier modifier)
            {
                if (path.Length < 2)
                    return;

                NativeList<float3> subdivided;

                if (modifier.UniformLength)
                {
                    float maxSegmentLength = math.max(modifier.MaxSegmentLength, 0.005f);

                    float pathLength = 0;
                    for (int i = 0; i < path.Length - 1; i++)
                    {
                        pathLength += math.distance(path[i].Value, path[i + 1].Value);
                    }

                    int estimatedNumberOfSegments = (int)math.floor(pathLength / maxSegmentLength);
                    subdivided = new NativeList<float3>(estimatedNumberOfSegments + 2, Allocator.Temp);

                    float distanceAlong = 0;

                    for (int i = 0; i < path.Length - 1; i++)
                    {
                        PathWaypoint start = path[i];
                        PathWaypoint end = path[i + 1];

                        float length = math.distance(start.Value, end.Value);

                        while (distanceAlong < length)
                        {
                            subdivided.Add(math.lerp(start.Value, end.Value, distanceAlong / length));
                            distanceAlong += maxSegmentLength;
                        }

                        distanceAlong -= length;
                    }

                    subdivided.Add(path[path.Length - 1].Value);
                }
                else
                {
                    int subdivisions = math.max(modifier.Subdivisions, 0);

                    if (subdivisions > 10)
                        subdivisions = 10;

                    int steps = 1 << subdivisions;

                    subdivided = new NativeList<float3>((path.Length - 1) * steps + 1, Allocator.Temp);
                    Subdivide(path, subdivided, steps);
                }

                if (modifier.Strength > 0)
                {
                    for (int it = 0; it < modifier.Iterations; it++)
                    {
                        float3 prev = subdivided[0];

                        for (int i = 1; i < subdivided.Length - 1; i++)
                        {
                            float3 tmp = subdivided[i];

                            subdivided[i] = math.lerp(tmp, (prev + subdivided[i + 1]) / 2F, modifier.Strength);

                            prev = tmp;
                        }
                    }
                }

                path.Clear();
                path.EnsureCapacity(subdivided.Length);
                for (int i = 0; i < subdivided.Length; i++)
                {
                    path.Add(new PathWaypoint { Value = subdivided[i] });
                }

                subdivided.Dispose();
            }

            private void ApplyBezierModifier(in Seeker seeker, ref DynamicBuffer<PathWaypoint> path, in SmoothModifier modifier)
            {
                int subdivisions = modifier.Subdivisions;

                if (subdivisions < 0)
                    subdivisions = 0;

                int subMult = 1 << subdivisions;

                NativeList<float3> subdivided = new NativeList<float3>(Allocator.Temp);

                for (int i = 0; i < path.Length - 1; i++)
                {
                    float3 tangent1;
                    float3 tangent2;

                    if (i == 0)
                        tangent1 = path[i + 1].Value - path[i].Value;
                    else
                        tangent1 = path[i + 1].Value - path[i - 1].Value;

                    if (i == path.Length - 2)
                        tangent2 = path[i].Value - path[i + 1].Value;
                    else
                        tangent2 = path[i].Value - path[i + 2].Value;

                    tangent1 *= modifier.BezierTangentLength;
                    tangent2 *= modifier.BezierTangentLength;

                    float3 v1 = path[i].Value;
                    float3 v2 = v1 + tangent1;
                    float3 v4 = path[i + 1].Value;
                    float3 v3 = v4 + tangent2;

                    for (int j = 0; j < subMult; j++)
                    {
                        subdivided.Add(CubicBezier(v1, v2, v3, v4, (float)j / subMult));
                    }
                }

                subdivided.Add(path[path.Length - 1].Value);

                path.Clear();
                path.EnsureCapacity(subdivided.Length);
                for (int i = 0; i < subdivided.Length; i++)
                {
                    path.Add(new PathWaypoint { Value = subdivided[i] });
                }

                subdivided.Dispose();
            }

            private void ApplyOffsetModifier(in Seeker seeker, ref DynamicBuffer<PathWaypoint> path, in SmoothModifier modifier)
            {
                if (path.Length <= 2 || modifier.Iterations <= 0)
                    return;

                int maxLength = (path.Length - 2) * (int)math.pow(2, modifier.Iterations) + 2;

                NativeList<float3> subdivided = new NativeList<float3>(maxLength, Allocator.Temp);
                NativeList<float3> subdivided2 = new NativeList<float3>(maxLength, Allocator.Temp);

                for (int i = 0; i < maxLength; i++)
                {
                    subdivided.Add(float3.zero);
                    subdivided2.Add(float3.zero);
                }

                for (int i = 0; i < path.Length; i++)
                {
                    subdivided[i] = path[i].Value;
                }

                for (int iteration = 0; iteration < modifier.Iterations; iteration++)
                {
                    int currentPathLength = (path.Length - 2) * (int)math.pow(2, iteration) + 2;

                    (subdivided, subdivided2) = (subdivided2, subdivided);

                    const float nextMultiplier = 1F;

                    for (int i = 0; i < currentPathLength - 1; i++)
                    {
                        float3 current = subdivided2[i];
                        float3 next = subdivided2[i + 1];

                        float3 normal = math.cross(next - current, new float3(0, 1, 0));
                        normal = math.normalizesafe(normal);

                        bool firstRight = false;
                        bool secondRight = false;
                        bool setFirst = false;
                        bool setSecond = false;
                        if (i != 0 && !IsColinearXZ(current, next, subdivided2[i - 1]))
                        {
                            setFirst = true;
                            firstRight = RightOrColinearXZ(current, next, subdivided2[i - 1]);
                        }

                        if (i < currentPathLength - 1 && !IsColinearXZ(current, next, subdivided2[i + 2]))
                        {
                            setSecond = true;
                            secondRight = RightOrColinearXZ(current, next, subdivided2[i + 2]);
                        }

                        if (setFirst)
                        {
                            subdivided[i * 2] =
                                current + (firstRight ? normal * modifier.Offset * nextMultiplier : -normal * modifier.Offset * nextMultiplier);
                        }
                        else
                            subdivided[i * 2] = current;

                        if (setSecond)
                        {
                            subdivided[i * 2 + 1] =
                                next + (secondRight ? normal * modifier.Offset * nextMultiplier : -normal * modifier.Offset * nextMultiplier);
                        }
                        else
                            subdivided[i * 2 + 1] = next;
                    }

                    subdivided[(path.Length - 2) * (int)math.pow(2, iteration + 1) + 2 - 1] = subdivided2[currentPathLength - 1];
                }

                path.Clear();
                path.EnsureCapacity(subdivided.Length);
                for (int i = 0; i < subdivided.Length; i++)
                {
                    path.Add(new PathWaypoint { Value = subdivided[i] });
                }

                subdivided.Dispose();
                subdivided2.Dispose();
            }

            private void ApplyCurvedNonuniformModifier(in Seeker seeker, ref DynamicBuffer<PathWaypoint> path, in SmoothModifier modifier)
            {
                if (modifier.MaxSegmentLength <= 0)
                    return;

                int pointCounter = 0;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    float dist = math.length(path[i].Value - path[i + 1].Value);
                    for (float t = 0; t <= dist; t += modifier.MaxSegmentLength)
                    {
                        pointCounter++;
                    }
                }

                NativeList<float3> subdivided = new NativeList<float3>(pointCounter, Allocator.Temp);

                float3 preEndVel = math.length(path[1].Value - path[0].Value);

                for (int i = 0; i < path.Length - 1; i++)
                {
                    float dist = math.length(path[i].Value - path[i + 1].Value);

                    float3 startVel1 = preEndVel;
                    float3 endVel1 = i < path.Length - 2
                        ? math.length(math.length(path[i + 2].Value - path[i + 1].Value) -
                                      math.length(path[i].Value - path[i + 1].Value))
                        : math.length(path[i + 1].Value - path[i].Value);

                    float3 startVel = startVel1 * dist * modifier.Factor;
                    float3 endVel = endVel1 * dist * modifier.Factor;

                    float3 start = path[i].Value;
                    float3 end = path[i + 1].Value;

                    float onedivdist = 1F / dist;

                    for (float t = 0; t <= dist; t += modifier.MaxSegmentLength)
                    {
                        float t2 = t * onedivdist;

                        subdivided.Add(GetPointOnCubic(start, end, startVel, endVel, t2));
                    }

                    preEndVel = endVel1;
                }

                subdivided[subdivided.Length - 1] = path[path.Length - 1].Value;

                path.Clear();
                path.EnsureCapacity(subdivided.Length);
                for (int i = 0; i < subdivided.Length; i++)
                {
                    path.Add(new PathWaypoint { Value = subdivided[i] });
                }

                subdivided.Dispose();
            }

            private void Subdivide(DynamicBuffer<PathWaypoint> pathWaypoints, NativeList<float3> result, int subSegments)
            {
                for (int i = 0; i < pathWaypoints.Length - 1; i++)
                for (int j = 0; j < subSegments; j++)
                {
                    result.Add(math.lerp(pathWaypoints[i].Value, pathWaypoints[i + 1].Value, j / (float)subSegments));
                }

                result.Add(pathWaypoints[pathWaypoints.Length - 1].Value);
            }

            private float3 CubicBezier(float3 p0, float3 p1, float3 p2, float3 p3, float t)
            {
                t = math.clamp(t, 0, 1);
                float t2 = 1 - t;
                return t2 * t2 * t2 * p0 + 3 * t2 * t2 * t * p1 + 3 * t2 * t * t * p2 + t * t * t * p3;
            }

            private bool IsColinearXZ(float3 a, float3 b, float3 c)
            {
                float v = (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);

                return v <= 0.0000001f && v >= -0.0000001f;
            }

            private bool RightOrColinearXZ(float3 a, float3 b, float3 p) => (b.x - a.x) * (p.z - a.z) - (p.x - a.x) * (b.z - a.z) <= 0;

            private float3 GetPointOnCubic(float3 a, float3 b, float3 tan1, float3 tan2, float t)
            {
                float t2 = t * t, t3 = t2 * t;

                float h1 = 2 * t3 - 3 * t2 + 1; // calculate basis function 1
                float h2 = -2 * t3 + 3 * t2; // calculate basis function 2
                float h3 = t3 - 2 * t2 + t; // calculate basis function 3
                float h4 = t3 - t2; // calculate basis function 4

                return h1 * a + // multiply and sum all funtions
                       h2 * b + // together to build the interpolated
                       h3 * tan1 + // point along the curve.
                       h4 * tan2;
            }
        }
    }
}