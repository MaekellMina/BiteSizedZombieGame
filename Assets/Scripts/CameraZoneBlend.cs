using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoneBlend : MonoBehaviour
{

    public CinemachineVirtualCamera targetCamera;
    public int targetPriority = 20;
    public int defaultPriority = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetCamera.Priority = targetPriority;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetCamera.Priority = defaultPriority;
        }
    }
}
