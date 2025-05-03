using UnityEditor;
using UnityEngine;

public class TextureRenamer : EditorWindow
{
    [MenuItem("Tools/Rename Card Textures")]
    static void RenameCardTextures()
    {
        string[] suitNames = { "Hearts", "Clubs", "Diamonds", "Spades" };
        string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

        // Load all textures in Resources/card_faces
        string[] texturePaths = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/card_faces" });

        // Check that the correct number of textures are available
        if (texturePaths.Length != 52)
        {
            Debug.LogError("There should be 52 textures in the card_faces folder.");
            return;
        }

        int textureIndex = 0;

        // Iterate over suits and ranks to rename textures
        foreach (string suit in suitNames)
        {
            foreach (string rank in ranks)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(texturePaths[textureIndex]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

                // Rename the texture
                string newName = rank + "_of_" + suit;
                AssetDatabase.RenameAsset(texturePath, newName);

                textureIndex++;
            }
        }

        // Refresh the asset database to reflect changes
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Textures renamed successfully!");
    }
}