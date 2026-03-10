using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;

namespace Plugins.SimpleWallCollider.Editor
{
    [CustomEditor(typeof(WallCollider))]
    public class WallColliderInspector : UnityEditor.Editor
    {
        private SerializedProperty _invertProperty;
        private SerializedProperty _qualityProperty;
        private SerializedProperty _baseOffsetProperty;
        private SerializedProperty _heightProperty;
        private SerializedProperty _simplificationProperty;
        private SerializedProperty _groundProjectionLayerProperty;

        private SerializedProperty _gizmosDrawTypeProperty;
        private SerializedProperty _gizmosColorProperty;

        private void OnEnable()
        {
            EditorSplineUtility.AfterSplineWasModified += OnSplineChanged;

            _invertProperty = serializedObject.FindProperty("_invert");
            _qualityProperty = serializedObject.FindProperty("_quality");
            _baseOffsetProperty = serializedObject.FindProperty("_baseOffset");
            _heightProperty = serializedObject.FindProperty("_height");
            _simplificationProperty = serializedObject.FindProperty("_simplification");
            _groundProjectionLayerProperty = serializedObject.FindProperty("_groundProjectionLayer");

            _gizmosDrawTypeProperty = serializedObject.FindProperty("_gizmosDrawType");
            _gizmosColorProperty = serializedObject.FindProperty("_gizmosColor");
        }

        private void OnDisable()
        {
            EditorSplineUtility.AfterSplineWasModified -= OnSplineChanged;
        }

        private void OnSplineChanged(Spline spline)
        {
            if (target is not WallCollider wallCollider)
                return;

            wallCollider.OnSplineChanged(spline);
        }

        public override void OnInspectorGUI()
        {
            if (target is not WallCollider)
                return;

            DrawFields();
            DrawButtons();
            DrawColliderInfo();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFields()
        {
            EditorGUILayout.PropertyField(_invertProperty);
            EditorGUILayout.PropertyField(_qualityProperty);
            EditorGUILayout.PropertyField(_baseOffsetProperty);
            EditorGUILayout.PropertyField(_heightProperty);
            EditorGUILayout.PropertyField(_simplificationProperty);
            EditorGUILayout.PropertyField(_groundProjectionLayerProperty);
            EditorGUILayout.PropertyField(_gizmosDrawTypeProperty);

            if (_gizmosDrawTypeProperty.enumValueIndex != (int)GizmosDrawType.None)
                EditorGUILayout.PropertyField(_gizmosColorProperty);
        }

        private void DrawButtons()
        {
            GUILayout.Space(25);
            DrawGenerateColliderButton();
            GUILayout.Space(5);
            DrawProjectOnGroundButton();
            GUILayout.Space(5);
            DrawMeshRendererButton();
        }

        private void DrawGenerateColliderButton()
        {
            if (GUILayout.Button("Generate Collider"))
                ((WallCollider)target).GenerateCollider();
        }

        private void DrawProjectOnGroundButton()
        {
            if (GUILayout.Button("Project On Ground"))
                ((WallCollider)target).ProjectOnGround();
        }

        private void DrawMeshRendererButton()
        {
            WallCollider wallCollider = (WallCollider)target;

            bool isRendererSetup = wallCollider.TryGetComponent(out MeshRenderer _);

            if (isRendererSetup)
            {
                if (GUILayout.Button("Remove Nav Mesh Configuration", new GUIStyle(GUI.skin.button) { normal = { textColor = Color.red } }))
                {
                    DestroyImmediate(wallCollider.GetComponent<MeshFilter>());
                    DestroyImmediate(wallCollider.GetComponent<MeshRenderer>());
                    EditorUtility.SetDirty(wallCollider);
                }
            }
            else
            {
                if (GUILayout.Button("Configure for Nav Mesh"))
                {
                    if (wallCollider.gameObject.TryGetComponent(out MeshFilter _) == false)
                        wallCollider.gameObject.AddComponent<MeshFilter>();

                    MeshRenderer renderer = wallCollider.gameObject.AddComponent<MeshRenderer>();
                    renderer.SetMaterials(new List<Material>());
                    wallCollider.GenerateCollider();
                    EditorUtility.SetDirty(wallCollider);
                }
            }
        }

        private void DrawColliderInfo()
        {
            GUILayout.Space(20);
            if (((WallCollider)target).TryGetComponent(out MeshCollider collider) && collider.sharedMesh != null)
            {
                Mesh mesh = collider.sharedMesh;

                DrawLabelOnCenter($"Vertices Count: {mesh.vertexCount}");
                DrawLabelOnCenter($"Triangles Count: {mesh.triangles.Length / 3}");
            }
            else
            {
                DrawLabelOnCenter("No mesh collider or mesh available.");
            }

            void DrawLabelOnCenter(string text)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(text);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
    }
}