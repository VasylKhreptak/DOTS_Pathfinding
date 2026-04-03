using Entities.Authoring.Pathfinding;
using UnityEditor;
using UnityEngine.AI;

namespace Editor.Pathfinding
{
    [CustomEditor(typeof(AgentAuthoring))]
    [CanEditMultipleObjects]
    public class AgentAuthoringEditor : UnityEditor.Editor
    {
        private SerializedProperty _agentTypeProp;

        private void OnEnable()
        {
            _agentTypeProp = serializedObject.FindProperty("_agentType");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawAgentTypeField();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAgentTypeField()
        {
            int count = NavMesh.GetSettingsCount();
            string[] agentTypeNames = new string[count];
            int[] agentTypeIDs = new int[count];
            int selectedIndex = -1;

            for (int i = 0; i < count; i++)
            {
                NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(i);
                agentTypeIDs[i] = settings.agentTypeID;
                agentTypeNames[i] = NavMesh.GetSettingsNameFromID(settings.agentTypeID);

                if (agentTypeIDs[i] == _agentTypeProp.intValue)
                {
                    selectedIndex = i;
                }
            }

            EditorGUI.BeginChangeCheck();

            if (selectedIndex == -1 && count > 0)
                selectedIndex = 0;

            int newIndex = EditorGUILayout.Popup("Agent Type", selectedIndex, agentTypeNames);

            if (EditorGUI.EndChangeCheck())
            {
                if (newIndex >= 0 && newIndex < count)
                {
                    _agentTypeProp.intValue = agentTypeIDs[newIndex];
                }
            }

            DrawPropertiesExcluding(serializedObject, "_agentType", "m_Script");
        }
    }
}