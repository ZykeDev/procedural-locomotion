using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LocomotionSystem))]
public class LocomotionSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LocomotionSystem locomotionSystem = (LocomotionSystem)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);

        // ---------------------------------------------------
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent("Setup Model", "Setups the model so that it can be used by the Procedural Locomotion system.")))
        {
            locomotionSystem.SetupModel();
        }

        if (GUILayout.Button(new GUIContent("Reset Model", "Resets the model and removes the modules added by the Procedural Locomotion system.")))
        {
            locomotionSystem.ResetModel();
        }

        

        GUILayout.EndHorizontal();
        // ---------------------------------------------------

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Weights", EditorStyles.boldLabel);

        // ---------------------------------------------------
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent("Generate Weights", "Adds a Weight component to the Character's body and each of its limbs.")))
        {
            locomotionSystem.GenerateWeights();
        }

        if (GUILayout.Button(new GUIContent("Remove Weights", "Removes the Weight component from the Character's body and each of its limbs.")))
        {
            locomotionSystem.RemoveWeights();
        }

        GUILayout.EndHorizontal();
        // ---------------------------------------------------

    }
}
