using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DaydreamElements.ObjectManipulation;

public class MoveableObject : BaseInteractiveObject, IPointerDownHandler
{
    private LJCluster targetSystem;
    private OptimizationControl controlOpt;
    private bool wasClusterOptimizing;
    private int id;

    private Vector3 targetPosition;

    private Vector3 normalizedForward;
   
    private float targetControlZDistance;
    private float controlZDistance;

    private Quaternion targetOrientationDelta;
    private Quaternion orientationDelta;

    private float touchDownPositionY;
    /// The scale of touchpad motion from swipe applied in world units.
    [Tooltip("The scale of touchpad motion from swipe applied in world units.")]
    public float distanceIncrementOnSwipe = 2.0f;
    /// The minimum distance of the control transform from the controller.
    [Tooltip("The minimum distance of the control transform from the controller.")]
    public float distanceFromControllerMin = 0.5f;
    /// The maximum distance of the control transform from the controller.
    [Tooltip("The maximum distance of the control transform from the controller.")]
    public float distanceFromControllerMax = 50f;


    void Start()
    {
        if (targetSystem == null)
        {
            targetSystem = gameObject.GetComponentInParent<LJCluster>();
        }
        controlOpt = gameObject.GetComponentInParent<OptimizationControl>();
        id = int.Parse(gameObject.name.Substring(gameObject.name.IndexOf("_" )+1));
        //Debug.Log("this is object"+id);
    }

    protected override void OnSelect()
    {
        Vector3 vectorToObject = transform.position - ControlPosition;
        float d = vectorToObject.magnitude;

        base.OnSelect();
        ObjectManipulationPointer.SetSelected(gameObject.transform, Vector3.zero);

        targetPosition = transform.position;

        if (d > NORMALIZATION_EPSILON)
        {
            normalizedForward = vectorToObject / d;
        }
        else
        {
            d = 0;
            normalizedForward = ControlForward;
        }

        // Reset distance interpolation values to current values.
        targetControlZDistance = controlZDistance = d;
        // Reset orientation interpolation values to 0.
        targetOrientationDelta = orientationDelta = Quaternion.identity;

        targetSystem.isRotable = false;
        wasClusterOptimizing = controlOpt.IsClusterOptimizing();
        if (wasClusterOptimizing)
        {
            controlOpt.stopOptimizeCluster();
        } 

    }

    protected override void OnDeselect()
    {
        base.OnDeselect();
        ObjectManipulationPointer.ReleaseSelected(gameObject.transform);
 
        targetSystem.isRotable = true;
        if (wasClusterOptimizing)
        {
            controlOpt.runSteepDescent();
        }
    }

    protected override void OnDrag()
    {
        // On a new touch, record the start position.
        if (GvrControllerInput.TouchDown)
        {
            // Threshold and remap input to be -1 to 1.
            touchDownPositionY = GvrControllerInput.TouchPosCentered.y;
            // While touching, calculate the touchpad drag distance.
        }
        else if (GvrControllerInput.IsTouching)
        {
            ZDistanceFromSwipe();
        }

        targetOrientationDelta = ControlRotation * InverseControllerOrientation;
        orientationDelta = targetOrientationDelta;
        targetPosition = controlZDistance * (orientationDelta * normalizedForward) + ControlPosition;

        targetSystem.updateOneParticleGlobal(targetPosition, id);
    }

    private void ZDistanceFromSwipe()
    {
        // Compute delta position since last frame.
        float deltaPosition = GvrControllerInput.TouchPosCentered.y - touchDownPositionY;
        touchDownPositionY = GvrControllerInput.TouchPosCentered.y;

        // Set target distance based on touchpad delta.
        targetControlZDistance += deltaPosition * distanceIncrementOnSwipe;
        targetControlZDistance = Mathf.Clamp(targetControlZDistance,
                                             distanceFromControllerMin,
                                             distanceFromControllerMax);

        controlZDistance = targetControlZDistance;
    }

}
