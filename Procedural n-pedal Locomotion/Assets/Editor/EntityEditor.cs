using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Entity))]
public class EntityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Entity entity = (Entity)target;


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
    }
}
