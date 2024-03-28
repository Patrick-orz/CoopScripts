using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BoundCamera : CinemachineExtension
{
    [Tooltip("Bound Camera to minX~maxX")]
    public float minX,maxX;

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime){
      if(stage == CinemachineCore.Stage.Body){
        var pos = state.RawPosition;

        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = Camera.main.aspect * halfHeight;

        pos.x = Mathf.Clamp(pos.x,minX+halfWidth,maxX-halfWidth);
        state.RawPosition = pos;
      }
    }
}
