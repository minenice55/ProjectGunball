using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public static class CinemachineSwitcher
{
    public static void SwitchTo(CinemachineVirtualCamera cam)
    {
        var brain = CinemachineCore.Instance.FindPotentialTargetBrain(cam);
        if (brain != null)
        {
            brain.ActiveVirtualCamera.Priority = 0;
            cam.Priority = 1;
        }
    }
}
