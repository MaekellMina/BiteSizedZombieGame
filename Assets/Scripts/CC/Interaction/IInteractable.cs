using UnityEngine;


namespace cc.Interaction
{
    public interface IInteractable
    {
        public void OnInteract();
        public GameObject GetTargetObject();
    }
}


