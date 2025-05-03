using UnityEngine;
using UnityEngine.VFX;

[System.Serializable]
public class VFXTrigger : MonoBehaviour
{
    public VisualEffect vfx;

    void Start()
    {
        vfx.Play(); // Triggers "OnPlay" event
    }
}
