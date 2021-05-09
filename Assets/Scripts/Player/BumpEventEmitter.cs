using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumpEventEmitter : MonoBehaviour
{
    [SerializeField, FMODUnity.EventRef]
    private string playerBumpEvent;

    private void OnCollisionEnter(Collision other)
    {
        if (string.IsNullOrEmpty(playerBumpEvent)) return;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("BumpForce", other.relativeVelocity.y);
        FMODUnity.RuntimeManager.PlayOneShot(playerBumpEvent);
    }
}
