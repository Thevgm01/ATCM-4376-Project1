using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public static class CameraShaker
{
    static CinemachineImpulseSource impulse;

    public static void Shake()
    {
        if (impulse == null) impulse = GameObject.FindObjectOfType<CinemachineImpulseSource>();
        impulse.GenerateImpulse();
    }
}
