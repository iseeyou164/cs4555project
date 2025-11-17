using UnityEngine;
using UnityEditor;

public class ApplyKnightMaterial : EditorWindow
{
    [MenuItem("Tools/Apply Knight Material")]
    public static void ShowWindow()
    {
        GetWindow<ApplyKnightMaterial>("Apply Knight Material");
    }

    private void OnGUI()
    {
        GUILayout.Label("Apply DemoTexture Material to Knights", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Apply Material to All Knights in Scene"))
        {
            ApplyMaterialToKnights();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This will find all knight models in the scene and apply the DemoTexture material to them.", MessageType.Info);
    }

    private void ApplyMaterialToKnights()
    {
        // Load the material
        Material knightMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Toon_RTS_demo/models/Materials/DemoTexture.mat");

        if (knightMaterial == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Could not find DemoTexture material at:\nAssets/Toon_RTS_demo/models/Materials/DemoTexture.mat", 
                "OK");
            return;
        }

        // Find all GameObjects with "Knight" in the name or with the knight mesh
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            // Check if this is a knight model (by name or by mesh)
            bool isKnight = obj.name.Contains("Knight") || 
                           obj.name.Contains("ToonRTS") || 
                           obj.name.Contains("WK_HeavyIntantry");

            if (isKnight)
            {
                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                SkinnedMeshRenderer skinnedRenderer = obj.GetComponent<SkinnedMeshRenderer>();

                if (renderer != null)
                {
                    Material[] materials = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = knightMaterial;
                    }
                    renderer.sharedMaterials = materials;
                    count++;
                }
                else if (skinnedRenderer != null)
                {
                    Material[] materials = new Material[skinnedRenderer.sharedMaterials.Length];
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = knightMaterial;
                    }
                    skinnedRenderer.sharedMaterials = materials;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            EditorUtility.DisplayDialog("Success", 
                $"Applied DemoTexture material to {count} knight object(s) in the scene!", 
                "OK");
            Debug.Log($"Applied DemoTexture material to {count} knight object(s)");
        }
        else
        {
            EditorUtility.DisplayDialog("Info", 
                "No knight objects found in the scene. Make sure you have knight models added to the scene.", 
                "OK");
        }
    }
}

