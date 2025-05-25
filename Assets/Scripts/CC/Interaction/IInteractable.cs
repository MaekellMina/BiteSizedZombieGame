using UnityEngine;


namespace cc.Interaction.Interface
{
    public interface IInteractable
    {
        public void OnInteract();
        public GameObject GetTargetObject();
    }
}


