using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour
{
	public GameObject[] characters;
	public int selectedCharacter = 0;

	public void NextCharacter()
	{
		characters[selectedCharacter].SetActive(false);
		selectedCharacter = (selectedCharacter + 1) % characters.Length;
		characters[selectedCharacter].SetActive(true);
	}

	public void PreviousCharacter()
	{
		characters[selectedCharacter].SetActive(false);
		selectedCharacter--;
		if (selectedCharacter < 0)
		{
			selectedCharacter += characters.Length;
		}
		characters[selectedCharacter].SetActive(true);
	}

	public void StartGame()
	{
		PlayerPrefs.SetInt("selectedCharacter", selectedCharacter);
		SceneManager.LoadScene(1, LoadSceneMode.Single);
	}

	void Start()
	{
		// Ensure each character GameObject has a CharacterClickLoader configured
		for (int i = 0; i < characters.Length; i++)
		{
			CharacterClickLoader loader = characters[i].GetComponent<CharacterClickLoader>();
			if (loader == null)
			{
				loader = characters[i].AddComponent<CharacterClickLoader>();
			}
			loader.characterIndex = i;
			loader.gameplayScene = "game"; // Adjust if your gameplay scene has a different name
			// Add a collider if the model doesn\'t already have one so it can receive clicks
			if (characters[i].GetComponent<Collider>() == null && characters[i].GetComponentInChildren<Collider>() == null)
			{
				characters[i].AddComponent<BoxCollider>();
			}
		}
	}
}
