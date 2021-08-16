using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Entity))]
public class EntityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Entity entity = (Entity)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Weights", EditorStyles.boldLabel);

        // ---------------------------------------------------
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent("Generate Weights", "Adds a Weight component to the Entity's body and each of its limbs.")))
        {
            entity.GenerateWeights();
        }

        if (GUILayout.Button(new GUIContent("Remove Weights", "Removes the Weight component from the Entity's body and each of its limbs.")))
        {
            entity.RemoveWeights();
        }

        GUILayout.EndHorizontal();
        // ---------------------------------------------------

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Limb Constraints", EditorStyles.boldLabel);

        // ---------------------------------------------------
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent("Generate Constraints", "Adds an IK Manager and generates a constriant for each limb.")))
        {
            entity.GenerateConstraints();
        }

        if (GUILayout.Button(new GUIContent("Remove Constraints", "Adds the IK Manager.")))
        {
            //entity.();
            Debug.LogWarning("Unimplemented");
        }

        GUILayout.EndHorizontal();
        // ---------------------------------------------------

    }
}
