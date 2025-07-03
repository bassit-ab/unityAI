using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneButtonLoader : MonoBehaviour
{
    [Tooltip("Name of the scene to load when this button is pressed")]
    public string sceneName = "selection";

    void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(Load);
    }

    void Load()
    {
        SceneManager.LoadScene(sceneName);
    }
}