using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    public class CharacterButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private CharacterSelector selector;

        public void OnPointerClick(PointerEventData eventData)
        {
            if(selector != null && characterPrefab != null)
                selector.SelectCharacter(characterPrefab);
        }
    }
}