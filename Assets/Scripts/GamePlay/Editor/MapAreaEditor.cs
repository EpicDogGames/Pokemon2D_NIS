using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapArea))]

public class MapAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        int totalChance = serializedObject.FindProperty("totalChance").intValue;

        var style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;

        GUILayout.Label($"Total Chance = {totalChance}", style);

        if (totalChance != 100)
            EditorGUILayout.HelpBox("Total Chance is not 100", MessageType.Error);
    }
}
