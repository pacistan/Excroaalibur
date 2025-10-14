using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;


/** Manage All the Camera and Priority in the Game */
[ExecuteAlways]
public class GCameraManager : CinemachineCameraManagerBase
{
    protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime)
    {
        // get Highest Priority Camera
        CinemachineVirtualCameraBase highest = null;
        foreach (var cam in ChildCameras)
        {
            if (!cam.isActiveAndEnabled) continue;
            if (highest == null || cam.Priority > highest.Priority)
            {
                highest = cam;
            }
        }
        
        return highest;
    }
}
