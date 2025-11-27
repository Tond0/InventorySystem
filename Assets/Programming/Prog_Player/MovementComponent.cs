using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class MovementComponent
{
    [SerializeField, Tooltip("If this is set to 0, it will inherit the maxSpeed from the previous State")] public float maxSpeed;
    [SerializeField] public float maxAcceleration;
    [SerializeField] public float maxDeceleration;
    [SerializeField, Tooltip("The curve which decides how much boost to the acceleration we need to apply relate to the direction we want to move to and we're actually moving to. (t = 0 => we want to move in the opposite direction, t = 1 => we're moving in the same direction")] public AnimationCurve accelerationFactor;
    [SerializeField, Tooltip("Max camera angle when turning left and right")] public float maxCameraDutch = 1;

    [NonSerialized] public float accelerationMultiplier = 1;

    public void Move(Transform targetTransform, Rigidbody rb, Vector3 direction)
    {
        //The current velocity
        Vector3 currentVelocity = rb.linearVelocity;

        //Desire velocity relative to the camera
        Vector3 desireVelocity = new Vector3(direction.x, 0, direction.z) * maxSpeed;

        float maxStepAcceleration;
        //If we're moving
        if (direction != Vector3.zero)
        {
            //Get the dot product of where we want to go and where we are actually going.
            float velDot = Vector3.Dot(currentVelocity.normalized, desireVelocity.normalized);
            //Use the velocityDot to know how much we need to boost acceleration to instantly (or almost) go to the opposide direction without sliding
            maxStepAcceleration = maxAcceleration * accelerationFactor.Evaluate(velDot);
        }
        //If we're not moving we just use the deceleration variable
        else
        {
            maxStepAcceleration = maxDeceleration;
        }

        
        //The max acceleration that can be handle this frame
        float maxSpeedChange = maxStepAcceleration * Time.deltaTime * accelerationMultiplier;

        //Update the velocity
        Vector3 finalVelocity = Vector3.MoveTowards(currentVelocity, desireVelocity, maxSpeedChange);

        //We don't want to edit the y value of the velocity
        finalVelocity.y = rb.linearVelocity.y;

        //Apply new velocity
        rb.linearVelocity = finalVelocity;

        //DEBUG
        //The visual rapresentation of where we'd like to go
        Debug.DrawRay(targetTransform.position, desireVelocity, Color.red);
        //The visual rapresentation of where we're actually moving to
        Debug.DrawRay(targetTransform.position, finalVelocity, Color.green);
    }

    /// <summary>
    /// Calculate the relative camera direction, (I know transform.InverseTransformDirection does exist, I'm just testing myself here)
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="cameraTransform"></param>
    /// <returns></returns>
    public Vector3 GetCameraRelativeDirection(Vector2 direction, Transform cameraTransform)
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 cameraRelativeForward = direction.y * camForward;
        Vector3 cameraRelativeRight = direction.x * camRight;

        Vector3 cameraRelativeMovement = cameraRelativeForward + cameraRelativeRight;
        return cameraRelativeMovement;
    }
}