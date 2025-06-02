using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Collectible))]
public class CollectibleEditor : Editor
{
    SerializedProperty playerTagProp;
    SerializedProperty damageAmountProp; 

    private void OnEnable()
    {
        playerTagProp = serializedObject.FindProperty("playerTag");
        damageAmountProp = serializedObject.FindProperty("damageAmount"); 
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Acide Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(playerTagProp, new GUIContent("Player Tag", "Le tag de l'objet joueur qui sera affecté."));

        EditorGUILayout.PropertyField(damageAmountProp, new GUIContent("Damage Amount", "La quantité de dégâts infligés par l'acide."));

        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
    }
}