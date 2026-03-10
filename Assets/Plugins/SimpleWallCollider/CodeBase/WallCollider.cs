using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace Plugins.SimpleWallCollider
{
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(MeshCollider))]
    public class WallCollider : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private MeshCollider _meshCollider;

        [Header("Collider Preferences")]
        [SerializeField] private bool _invert;
        [SerializeField, Range(0.05f, 5f)] private float _quality = 1;
        [SerializeField] private float _baseOffset = 0f;
        [SerializeField, Min(0f)] private float _height = 5;
        [SerializeField, Range(0f, 10f)] private float _simplification = 1f;
        [SerializeField] private LayerMask _groundProjectionLayer = ~0;

        [Header("Visual Preferences")]
        [SerializeField] private GizmosDrawType _gizmosDrawType = GizmosDrawType.Always;
        [SerializeField] private Color _gizmosColor = new Color(0f, 1f, 0, 0.7f);

        #region MonoBehaviour

        private void OnValidate()
        {
            _splineContainer = GetComponent<SplineContainer>();
            _meshCollider = GetComponent<MeshCollider>();

            GenerateCollider();
        }

        #endregion

        public void OnSplineChanged(Spline spline)
        {
            if (_splineContainer.Splines.Contains(spline) == false)
                return;

            GenerateCollider();
        }

        public void GenerateCollider()
        {
            MeshFilter filter = GetComponent<MeshFilter>();

            if (_height == 0)
            {
                _meshCollider.sharedMesh = null;

                if (filter != null)
                    filter.sharedMesh = null;

                return;
            }

            Mesh[] meshes = new Mesh[_splineContainer.Splines.Count];

            for (int i = 0; i < _splineContainer.Splines.Count; i++)
                meshes[i] = GenerateMesh(_splineContainer.Splines[i]);

            Mesh combinedMesh = CombineMeshes(meshes);
            combinedMesh.name = "Wall collider";

            _meshCollider.sharedMesh = combinedMesh;

            if (filter != null)
                filter.sharedMesh = combinedMesh;
        }

        public void ProjectOnGround()
        {
            for (int i = 0; i < _splineContainer.Splines.Count; i++)
            {
                Spline spline = _splineContainer.Splines[i];
                List<BezierKnot> knots = spline.Knots.ToList();

                for (int j = 0; j < knots.Count; j++)
                {
                    BezierKnot knot = knots[j];
                    Vector3 worldPosition = _splineContainer.transform.TransformPoint(knot.Position);

                    if (Physics.Raycast(worldPosition + Vector3.up * 500f, Vector3.down, out RaycastHit hit, 2000f, _groundProjectionLayer))
                    {
                        Vector3 localPosition = _splineContainer.transform.InverseTransformPoint(hit.point);
                        knot.Position = localPosition;

                        spline.SetKnot(j, knot);
                    }
                }
            }
        }

        private Mesh GenerateMesh(Spline spline)
        {
            List<Vector3> basePoints = CalculateBasePoints(spline);

            if (basePoints == null || basePoints.Count < 2)
                return new Mesh();

            Mesh mesh = new Mesh();

            int basePointCount = basePoints.Count;
            Vector3[] vertices = new Vector3[basePointCount * 2];
            for (int i = 0; i < basePointCount; i++)
            {
                Vector3 basePoint = basePoints[i];
                vertices[i] = basePoint;
                vertices[i + basePointCount] = basePoint + transform.up * _height;
            }

            List<int> triangles = new List<int>();
            for (int i = 0; i < basePointCount - 1; i++)
            {
                if (_invert)
                {
                    triangles.Add(i + 1);
                    triangles.Add(i + basePointCount);
                    triangles.Add(i);

                    triangles.Add(i + 1);
                    triangles.Add(i + basePointCount + 1);
                    triangles.Add(i + basePointCount);
                }
                else
                {
                    triangles.Add(i);
                    triangles.Add(i + basePointCount);
                    triangles.Add(i + 1);

                    triangles.Add(i + 1);
                    triangles.Add(i + basePointCount);
                    triangles.Add(i + basePointCount + 1);
                }
            }

            if (spline.Closed)
            {
                int lastIndex = basePointCount - 1;

                if (_invert)
                {
                    triangles.Add(0);
                    triangles.Add(lastIndex + basePointCount);
                    triangles.Add(lastIndex);

                    triangles.Add(0);
                    triangles.Add(basePointCount);
                    triangles.Add(lastIndex + basePointCount);
                }
                else
                {
                    triangles.Add(lastIndex);
                    triangles.Add(lastIndex + basePointCount);
                    triangles.Add(0);

                    triangles.Add(0);
                    triangles.Add(lastIndex + basePointCount);
                    triangles.Add(basePointCount);
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateNormals();

            if (_invert)
            {
                Vector3[] normals = mesh.normals;
                for (int i = 0; i < normals.Length; i++)
                {
                    normals[i] = -normals[i];
                }

                mesh.normals = normals;
            }

            mesh.RecalculateBounds();

            return mesh;
        }

        private List<Vector3> CalculateBasePoints(Spline spline)
        {
            if (spline.Knots.Count() < 2)
                return new List<Vector3>();

            List<Vector3> points = new List<Vector3>();

            float length = spline.GetLength();

            float timeStep = 1 / (length * _quality);

            for (float t = 0; t < 1; t += timeStep)
                points.Add(spline.EvaluatePosition(t));

            if (spline.Closed == false)
                points.Add(spline.EvaluatePosition(1));

            points = AddOffset(points);

            return Simplify(points, _simplification);
        }

        private List<Vector3> AddOffset(List<Vector3> points)
        {
            for (int i = 0; i < points.Count; i++)
                points[i] += transform.up * _baseOffset;

            return points;
        }

        private List<Vector3> Simplify(List<Vector3> points, float angleThreshold)
        {
            if (points.Count < 3 || angleThreshold == 0)
                return points;

            List<Vector3> simplified = new List<Vector3> { points[0] };

            float cosineThreshold = Mathf.Cos(angleThreshold * Mathf.Deg2Rad);

            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector3 prev = points[i - 1];
                Vector3 current = points[i];
                Vector3 next = points[i + 1];

                Vector3 dirToPrev = (current - prev).normalized;
                Vector3 dirToNext = (next - current).normalized;

                float dotProduct = Vector3.Dot(dirToPrev, dirToNext);

                if (dotProduct <= cosineThreshold)
                    simplified.Add(current);
            }

            simplified.Add(points[^1]);

            return simplified;
        }

        private Mesh CombineMeshes(params Mesh[] meshes)
        {
            Mesh combinedMesh = new Mesh
            {
                name = "Combined Mesh"
            };

            if (meshes.Length == 0)
                return combinedMesh;

            CombineInstance[] combine = new CombineInstance[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
            {
                combine[i].mesh = meshes[i];
                combine[i].transform = Matrix4x4.identity;
            }

            combinedMesh.CombineMeshes(combine, true, false);

            return combinedMesh;
        }

        private void OnDrawGizmos()
        {
            if (_gizmosDrawType == GizmosDrawType.Always)
                DrawGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (_gizmosDrawType == GizmosDrawType.Selected)
                DrawGizmos();
        }

        private void DrawGizmos()
        {
            if (_meshCollider == null || _meshCollider.sharedMesh == null)
                return;

            Gizmos.color = _gizmosColor;
            Mesh mesh = _meshCollider.sharedMesh;

            if (mesh.vertices.Length == 0)
                return;

            Gizmos.DrawMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
        }
    }
}