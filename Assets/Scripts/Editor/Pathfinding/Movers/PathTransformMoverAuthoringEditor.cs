using Entities.Authoring.Pathfinding.Movers;
using UnityEditor;

namespace Editor.Pathfinding.Movers
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PathTransformMoverAuthoring))]
    public class PathTransformMoverAuthoringEditor : UnityEditor.Editor
    {
        private SerializedProperty _canMove;
        private SerializedProperty _maxSpeed;
        private SerializedProperty _acceleration;
        private SerializedProperty _deceleration;
        private SerializedProperty _enableRotation;
        private SerializedProperty _rotationSpeed;
        private SerializedProperty _slowWhenNotFacingTarget;

        private SerializedProperty _pickNextWaypointDistance;
        private SerializedProperty _endReachedDistance;
        private SerializedProperty _slowdownDistance;

        private void OnEnable()
        {
            _canMove = serializedObject.FindProperty("_canMove");
            _maxSpeed = serializedObject.FindProperty("_maxSpeed");
            _acceleration = serializedObject.FindProperty("_acceleration");
            _deceleration = serializedObject.FindProperty("_deceleration");

            _enableRotation = serializedObject.FindProperty("_enableRotation");
            _rotationSpeed = serializedObject.FindProperty("_rotationSpeed");
            _slowWhenNotFacingTarget = serializedObject.FindProperty("_slowWhenNotFacingTarget");

            _pickNextWaypointDistance = serializedObject.FindProperty("_pickNextWaypointDistance");
            _endReachedDistance = serializedObject.FindProperty("_endReachedDistance");
            _slowdownDistance = serializedObject.FindProperty("_slowdownDistance");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_canMove);
            EditorGUILayout.PropertyField(_maxSpeed);
            EditorGUILayout.PropertyField(_acceleration);
            EditorGUILayout.PropertyField(_deceleration);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_enableRotation);

            if (_enableRotation.boolValue)
            {
                EditorGUILayout.PropertyField(_rotationSpeed);
                EditorGUILayout.PropertyField(_slowWhenNotFacingTarget);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Path Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_pickNextWaypointDistance);
            EditorGUILayout.PropertyField(_endReachedDistance);
            EditorGUILayout.PropertyField(_slowdownDistance);

            serializedObject.ApplyModifiedProperties();
        }
    }
}