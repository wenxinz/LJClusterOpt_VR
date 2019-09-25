using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterManipulationControl : MonoBehaviour {
    public LJCluster targetSystem;

    [Tooltip("The max angular velocity to rotate the cluster (degrees per second).")]
    //[Range(0.0f, 180.0f)]
    public float rotationCoeff = 25.0f;

    [Tooltip("Controls how far the user must touch on the touchpad to start rotate the cluster.")]
    //[Range(0.25f, 1.0f)]
    public float rotateInputThreshold = 2f;

    private Vector2? previousPos = null;
    private Vector2 currentPos;

    private int framecount = 0;

    // Update is called once per frame
    void Update () {
        if (targetSystem.isRotable)
        {
            if (GvrControllerInput.TouchDown)
            {
                previousPos = GvrControllerInput.TouchPosCentered;
            }
            else if (GvrControllerInput.IsTouching && previousPos != null)
            {
                    currentPos = GvrControllerInput.TouchPosCentered;
                    float deltaX = currentPos.x - ((Vector2)previousPos).x;
                    float deltaY = currentPos.y - ((Vector2)previousPos).y;
                    float euler_x = 0.0f;
                    float euler_y = 0.0f;
                    if (Mathf.Abs(Mathf.Max(deltaX/Time.deltaTime, deltaX/0.1f)) > rotateInputThreshold)
                    {
                        // rotate around y axis
                        euler_y = -deltaX;
                    }
                    if (Mathf.Abs(Mathf.Max(deltaX / Time.deltaTime, deltaX/0.1f)) > rotateInputThreshold)
                    {
                        // rotate around x axis
                        euler_x = deltaY;
                    }
                    Vector3 rotation = new Vector3(euler_x, euler_y, 0.0f);
                    Rotate(Vector3.Normalize(rotation));
                    previousPos = currentPos;
            }
            else if (GvrControllerInput.TouchUp)
            {
                previousPos = null;
            }
        }
    }

    private void Rotate(Vector3 rotation)
    {
        targetSystem.transform.Rotate(rotationCoeff*rotation, Space.World);
    }

}
