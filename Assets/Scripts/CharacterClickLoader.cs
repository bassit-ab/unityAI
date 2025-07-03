using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterClickLoader : MonoBehaviour
{
    [Tooltip("Index of this character in the characterPrefabs array used in the gameplay scene")] 
    public int characterIndex = 0;

    [Tooltip("Name of the gameplay scene to load")] 
    public string gameplayScene = "game";

    // Called when the player clicks this GameObject (requires Collider & appropriate EventSystem for UI)
    void OnMouseDown()
    {
        // Save chosen character index for the gameplay scene to read
        PlayerPrefs.SetInt("selectedCharacter", characterIndex);
        // Optionally persist immediately
        PlayerPrefs.Save();

        // Load gameplay scene
        SceneManager.LoadScene(gameplayScene);
    }
}
