using Entities.Authoring.Pathfinding.Modifiers;
using UnityEditor;

namespace Editor.Pathfinding
{
    [CustomEditor(typeof(SmoothModifierAuthoring))]
    [CanEditMultipleObjects]
    public class SmoothModifierAuthoringEditor : UnityEditor.Editor
    {
        private SerializedProperty _smoothType;
        private SerializedProperty _subdivisions;
        private SerializedProperty _iterations;
        private SerializedProperty _strength;
        private SerializedProperty _uniformLength;
        private SerializedProperty _maxSegmentLength;
        private SerializedProperty _bezierTangentLength;
        private SerializedProperty _offset;
        private SerializedProperty _factor;

        private void OnEnable()
        {
            _smoothType = serializedObject.FindProperty("_smoothType");
            _subdivisions = serializedObject.FindProperty("_subdivisions");
            _iterations = serializedObject.FindProperty("_iterations");
            _strength = serializedObject.FindProperty("_strength");
            _uniformLength = serializedObject.FindProperty("_uniformLength");
            _maxSegmentLength = serializedObject.FindProperty("_maxSegmentLength");
            _bezierTangentLength = serializedObject.FindProperty("_bezierTangentLength");
            _offset = serializedObject.FindProperty("_offset");
            _factor = serializedObject.FindProperty("_factor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_smoothType);

            SmoothType selectedType = (SmoothType)_smoothType.enumValueIndex;

            EditorGUILayout.Space(2);

            switch (selectedType)
            {
                case SmoothType.Simple:
                    EditorGUILayout.PropertyField(_uniformLength);
                    EditorGUILayout.PropertyField(_maxSegmentLength);
                    EditorGUILayout.PropertyField(_iterations);
                    EditorGUILayout.Slider(_strength, 0f, 1f, "Strength");
                    break;

                case SmoothType.Bezier:
                    EditorGUILayout.IntSlider(_subdivisions, 0, 8, "Subdivisions");
                    EditorGUILayout.PropertyField(_bezierTangentLength);
                    break;

                case SmoothType.OffsetSimple:
                    EditorGUILayout.PropertyField(_iterations);
                    EditorGUILayout.PropertyField(_offset);
                    break;

                case SmoothType.CurvedNonuniform:
                    EditorGUILayout.PropertyField(_maxSegmentLength);
                    EditorGUILayout.PropertyField(_factor);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}