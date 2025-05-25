using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace cc.Interaction.SO
{
    [CreateAssetMenu(menuName = "Signals/Interaction Signal")]
    [System.Serializable]
    public class InteractionSignalSO : ScriptableObject
    {
        private readonly UnityEvent<GameObject> interact_listeners = new UnityEvent<GameObject>();
        [SerializeField]
        private GameObject currentPlayer;
        [SerializeField]
        private GameObject interactionPrompt;
        public GameObject currentInteractable;
        public GameObject CurrentPlayer { get => currentPlayer; }

        public void AssignPlayer(GameObject player)
        {
            currentPlayer = player;
        }
        public void AssignPlayer(GameObject _player,GameObject _interactionPrompt)
        {
            currentPlayer =_player;
            interactionPrompt = _interactionPrompt;
        }
        public void Raise(GameObject Interactable)
        {
            Debug.Log($"attempt to raise {Interactable.name} {currentInteractable}");
            if(currentInteractable!=null)
            {
                Debug.Log($"Success");
                interact_listeners?.Invoke(Interactable);
            }
             
        }

        public void RegisterListener(UnityAction<GameObject> listener)
        {
            interact_listeners.AddListener(listener);
        }

        public void UnregisterListener(UnityAction<GameObject> listener)
        {
            interact_listeners.RemoveListener(listener);
        }

        public void SetCurrentInteractable(GameObject InteractableSender)
        {
            Debug.Log($"Current interactable is {InteractableSender.name}");
            PositionPrompt(InteractableSender);
            currentInteractable = InteractableSender;
        }

        public void ClearCurrentInteractable(GameObject InteractableSender)
        {
            if (InteractableSender == currentInteractable)
            {
                DisablePrompt();
                currentInteractable = null;
            }

        }
        void PositionPrompt(GameObject InteractableSender)
        {
            interactionPrompt.transform.position = InteractableSender.transform.position + Vector3.up;
            interactionPrompt.gameObject.SetActive(true);
        }
        
        public void DisablePrompt()
        {
            interactionPrompt.gameObject.SetActive(false);
        }

        public bool IsMyPlayer(GameObject Player) => Player == CurrentPlayer;
    }



}

