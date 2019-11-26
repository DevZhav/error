using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Player.Weapons.BulletPattern))]
public class BulletPatternEditor : Editor
{
    // The target script in serialized and non-serialized form
    Player.Weapons.BulletPattern targetScript;
    SerializedObject serializedTargetScript;

    SerializedProperty Bullets;
    SerializedProperty Pattern;
    float range;
    int bullets;

    private void OnEnable()
    {
        // Get a reference to the target script and serialize it
        targetScript = (Player.Weapons.BulletPattern)target;
        serializedTargetScript = new SerializedObject(targetScript);

        Bullets = serializedTargetScript.FindProperty("Bullets");
        Pattern = serializedTargetScript.FindProperty("Pattern");
    }

    public override void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedTargetScript.Update();
        EditorGUI.BeginChangeCheck();

        bullets = Bullets.intValue;
        bullets = EditorGUILayout.IntSlider("Number of Bullets", bullets, 0, 200);
        Bullets.intValue = bullets;

        Pattern.arraySize = Bullets.intValue;
        EditorGUILayout.PropertyField(Pattern, new GUIContent("Pattern"), true);
        range = EditorGUILayout.Slider("Random Range", range, 0, 3);

        if (GUILayout.Button("Random"))
        {
            for (int i = 0; i < Pattern.arraySize; i++)
            {
                Vector2 vec = Pattern.GetArrayElementAtIndex(i).vector2Value;
                vec.x = Random.Range(-range, range);
                vec.y = Random.Range(-range, range);
                Pattern.GetArrayElementAtIndex(i).vector2Value = vec;
                serializedTargetScript.ApplyModifiedProperties();
            }
        }
        if (GUILayout.Button("Reset"))
        {
            for (int i = 0; i < Pattern.arraySize; i++)
            {
                Vector2 vec = Pattern.GetArrayElementAtIndex(i).vector2Value;
                vec.x = 0;
                vec.y = 0;
                Pattern.GetArrayElementAtIndex(i).vector2Value = vec;
                serializedTargetScript.ApplyModifiedProperties();
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedTargetScript.ApplyModifiedProperties();
        }
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override GUIContent GetPreviewTitle()
    {
        return new GUIContent("Bullet Spray Preview");
    }

    // Helper method to get a square rectangle of the correct aspect ratio
    private Rect GetCenteredRect(Rect rect, float aspect = 1f)
    {
        Vector2 size = rect.size;
        size.x = Mathf.Min(size.x, rect.size.y * aspect);
        size.y = Mathf.Min(size.y, rect.size.x / aspect);

        Vector2 pos = rect.min + (rect.size - size) * 0.5f;
        return new Rect(pos, size);
    }

    Color32 low = new Color32(255, 10, 111, 255);
    Color32 high = new Color32(255, 114, 31, 255);
    public override void DrawPreview(Rect rect)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };

        rect = GetCenteredRect(rect);

        // Draw background of the rect we plot points in
        GraphBackground.DrawGraphBackground(rect, rect);
        //EditorGUI.DrawRect(rect, new Color(0.9f, 0.9f, 0.9f));

        float dotSize = 15; // size in pixels of the point we draw
        float halfDotSize = dotSize * 0.5f;

        float viewportSize = 5; // size of our viewport in Units
                                // a value of 10 means we can display any vector from -5,-5 to 5,5 within our rect.
                                // change this value for your needs

        for (int i = 0; i < Pattern.arraySize; i++)
        {
            SerializedProperty vectorProperty = Pattern.GetArrayElementAtIndex(i);

            Vector2 vector = new Vector2(vectorProperty.vector2Value.x, vectorProperty.vector2Value.y);
            Vector2 normalizedPosition = vector / new Vector2(viewportSize, -viewportSize);

            if (Mathf.Abs(normalizedPosition.x) > 0.5f || Mathf.Abs(normalizedPosition.y) > 0.5f)
            {
                // don't draw points outside our viewport
                continue;
            }

            float l = (float)i / (float)Pattern.arraySize;
            Color32 lerpedColor = Color32.Lerp(low, high, l);

            Vector2 pixelPosition = rect.center + rect.size * normalizedPosition;
            EditorGUI.DrawRect(new Rect(pixelPosition.x - halfDotSize, pixelPosition.y - halfDotSize, dotSize, dotSize), lerpedColor);
            EditorGUI.LabelField(new Rect(pixelPosition.x - halfDotSize, pixelPosition.y - halfDotSize, dotSize, dotSize), i.ToString(), style);
        }
    }
}
