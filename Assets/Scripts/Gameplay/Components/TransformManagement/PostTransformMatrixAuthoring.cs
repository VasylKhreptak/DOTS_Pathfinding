using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Gameplay.Components.TransformManagement
{
    public class PostTransformMatrixAuthoring : MonoBehaviour
    {
        private class PostTransformMatrixBaker : Baker<PostTransformMatrixAuthoring>
        {
            public override void Bake(PostTransformMatrixAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                PostTransformMatrix postTransformMatrix = new PostTransformMatrix
                {
                    Value = float4x4.Scale(1f)
                };

                AddComponent(entity, postTransformMatrix);
            }
        }
    }
}