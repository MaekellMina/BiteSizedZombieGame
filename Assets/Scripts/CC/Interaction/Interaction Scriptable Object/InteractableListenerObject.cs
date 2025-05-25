using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace cc.Interaction.SO
{
 [System.Serializable]
 public class PlayerInteractionSet
    {       
        public InteractionSignalSO interactionSignal;
        public GameObject Player => interactionSignal.CurrentPlayer;
        public void RegisterListener(UnityAction<GameObject> listener) => interactionSignal.RegisterListener(listener);
        public void UnregisterListener(UnityAction<GameObject> listener) => interactionSignal.UnregisterListener(listener);

        public bool IsMyPlayer(GameObject player)
        {
            return player == Player;
        }

    }
    public class InteractableListenerObject : MonoBehaviour
    {
        public List<PlayerInteractionSet> interactionSignals = new List<PlayerInteractionSet>();

        [System.Serializable]
        public class PlayerInteractEvent : UnityEvent<GameObject> { }

        public PlayerInteractEvent OnInteract;



        private void OnEnable()
        {
            if (interactionSignals.Count > 0)
                for (int i = 0; i < interactionSignals.Count; i++)
                {
                    interactionSignals[i].RegisterListener(HandleInteraction);

                }

        }

        private void OnDisable()
        {
            if (interactionSignals.Count > 0)
                for (int i = 0; i < interactionSignals.Count; i++)
                {
                    interactionSignals[i].UnregisterListener(HandleInteraction);

                }
        }

        private void HandleInteraction(GameObject Interactable)
        {
            if(Interactable == this.gameObject)
               OnInteract?.Invoke(this.gameObject);
        } 

        private bool IsPlayerInRange(GameObject player)
        {
            for (int i = 0; i < interactionSignals.Count; i++)
            {
                if (interactionSignals[i].IsMyPlayer(player))
                    return true;

            }
            return false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Player")
            {
                for (int i = 0; i < interactionSignals.Count; i++)
                {
                    if (interactionSignals[i].IsMyPlayer(other.gameObject))
                        interactionSignals[i].interactionSignal.SetCurrentInteractable(this.gameObject);

                }
            }
           
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "Player")
            {
                for (int i = 0; i < interactionSignals.Count; i++)
                {
                    if (interactionSignals[i].IsMyPlayer(other.gameObject))
                        interactionSignals[i].interactionSignal.ClearCurrentInteractable(this.gameObject);

                }
            }

        }
    }

}
