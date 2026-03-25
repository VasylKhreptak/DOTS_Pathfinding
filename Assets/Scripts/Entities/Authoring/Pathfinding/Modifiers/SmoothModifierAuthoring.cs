using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding.Modifiers
{
    [DisallowMultipleComponent]
    public class SmoothModifierAuthoring : MonoBehaviour
    {
        [SerializeField] private SmoothType _smoothType = SmoothType.Simple;
        [SerializeField] private int _subdivisions = 2;
        [SerializeField] private int _iterations = 2;
        [SerializeField] private float _strength = 0.5f;
        [SerializeField] private bool _uniformLength = true;
        [SerializeField] private float _maxSegmentLength = 2f;
        [SerializeField] private float _bezierTangentLength = 0.4f;
        [SerializeField] private float _offset = 0.2f;
        [SerializeField] private float _factor = 0.1f;

        private class SmoothModifierBaker : Baker<SmoothModifierAuthoring>
        {
            public override void Bake(SmoothModifierAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                SmoothModifier smoothModifier = new SmoothModifier()
                {
                    SmoothType = authoring._smoothType,
                    Subdivisions = authoring._subdivisions,
                    Iterations = authoring._iterations,
                    Strength = authoring._strength,
                    UniformLength = authoring._uniformLength,
                    MaxSegmentLength = authoring._maxSegmentLength,
                    BezierTangentLength = authoring._bezierTangentLength,
                    Offset = authoring._offset,
                    Factor = authoring._factor
                };

                AddComponent(entity, smoothModifier);
            }
        }
    }

    public struct SmoothModifier : IComponentData, IEnableableComponent
    {
        public SmoothType SmoothType;
        public int Subdivisions;
        public int Iterations;
        public float Strength;
        public bool UniformLength;
        public float MaxSegmentLength;
        public float BezierTangentLength;
        public float Offset;
        public float Factor;
    }

    public enum SmoothType
    {
        Simple,
        Bezier,
        OffsetSimple,
        CurvedNonuniform
    }
}