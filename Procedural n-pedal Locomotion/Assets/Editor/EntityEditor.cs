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
        EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);

        // ---------------------------------------------------
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent("Setup Model", "Setups the model so that it can be used by the Procedural Locomotion system.")))
        {
            entity.SetupModel();
        }

        if (GUILayout.Button(new GUIContent("Reset Model", "Resets the model and removes the modules added by the Procedural Locomotion system.")))
        {
            entity.ResetModel();
        }

        

        GUILayout.EndHorizontal();
        // ---------------------------------------------------

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

    }
}
