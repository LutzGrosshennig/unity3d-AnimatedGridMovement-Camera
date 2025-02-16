/**************************************************************
 * The AnimatedGridMovement script performs "Dungeon Master"/ * 
 * "Legend of Grimrock" style WSADEQ movement in your Unity3D *
 * game using a single file.                                  *
 *                                                            *
 * Written by: Lutz Grosshennig, 06/27/2020                   *
 * MIT Licence                                                *
 * ************************************************************/
using System;
using UnityEngine;

public class AnimatedGridMovement : MonoBehaviour
{
    private const float LeftHand = -90.0f;
    private const float RightHand = +90.0f;

    [Header("Grid settings")]
    [SerializeField] private float gridSize = 3.0f;

    [Header("Animation settings")]
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float movementSpeed = 5.0f;

    [Header("Free look settings")]
    [SerializeField] private float freelookSensitivity = 1.0f;
    [Range(45.0f, 89.0f)]
    [SerializeField] private float freelookAngle = 85.0f;
    [SerializeField] private float freelookSnapbackSpeed = 10.0f;

    // Used for the rotation animation
    private Quaternion rotateFromDirection;
    private Quaternion rotateTowardsDirection;
    private float rotationTime = 0.0f;

    // Used for the movement animation
    private Vector3 moveFromPosition;
    private Vector3 moveTowardsPosition;
    private float movementTime = 0.0f;

    // Used for the freelook + animation
    private bool freelookModeEnabled = false;
    private float freelookTime = 0.0f;
    private Quaternion freelookReturnToRotation;
    private Quaternion freelookStartFromRotation;

    void Update()
    {
        if (IsStationary())
        {
            // Personaly I would prefer a dictionary<KeyCode,Action> lookup but this does not seem possible with the old input system.
            if (Input.GetKey(KeyCode.W))
            {
                MoveForward();
            }
            else if (Input.GetKey(KeyCode.S))
            {
                MoveBackward();
            }
            else if (Input.GetKey(KeyCode.A))
            {
                StrafeLeft();
            }
            else if (Input.GetKey(KeyCode.D))
            {
                StrafeRight();
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                TurnLeft();
            }
            else if (Input.GetKey(KeyCode.E))
            {
                TurnRight();
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                EnterFreeLookMode();
            }
        }

        // Left mouse button released?
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            ExitFreeLookMode();
        }

        // Are we in free look mode?
        if (freelookModeEnabled)
        {
            DoFreeLook();
        }
        else
        {
            if (IsMoving())
            {
                AnimateMovement();
            }
            else if (IsRotating())
            {
                AnimateRotation();
            }
            else if (IsSnapBack())
            {
                AnimateSnapBack();
            }
        }
    }

    private void AnimateRotation()
    {
        rotationTime += Time.deltaTime * rotationSpeed;
        transform.rotation = Quaternion.Slerp(rotateFromDirection, rotateTowardsDirection, rotationTime);

        if (rotationTime >= 1.0f)
        {
            rotationTime = 0;
            transform.rotation = rotateTowardsDirection;
        }
    }

    private void AnimateMovement()
    {
        movementTime += Time.deltaTime * movementSpeed;
        transform.position = Vector3.Lerp(moveFromPosition, moveTowardsPosition, movementTime);

        if (movementTime >= 1.0f)
        {
            movementTime = 0.0f;
            transform.position = moveTowardsPosition;
        }
    }

    private void AnimateSnapBack()
    {
        freelookTime += Time.deltaTime * freelookSnapbackSpeed;
        transform.localRotation = Quaternion.Slerp(freelookStartFromRotation, freelookReturnToRotation, freelookTime);
        if (freelookTime >= 1.0f)
        {
            freelookTime = 0.0f;
            transform.localRotation = freelookReturnToRotation;
        }
    }

    private void DoFreeLook()
    {
        // calling this function causes a native call to the c++ side, so we only want to do that once and reuse the result.
        Vector3 localEulerAngles = transform.localEulerAngles;

        float lookAtAngle_Y = localEulerAngles.y + Input.GetAxis("Mouse X") * freelookSensitivity;
        float lookAtAngle_X = localEulerAngles.x - Input.GetAxis("Mouse Y") * freelookSensitivity;

        // calling this function causes a native call to the c++ side, so we only want to do that once and reuse the result.
        float viewDirection = rotateTowardsDirection.eulerAngles.y;

        float minClampRange = viewDirection - freelookAngle;
        float maxClampRange = viewDirection + freelookAngle;

        // Since we are in Euler space we need to prevent a "Gimble look", however Euler angles are modular so we
        // need to take this into account when we want to clamp the angles.
        lookAtAngle_X = ClampAngle(lookAtAngle_X, -freelookAngle, freelookAngle);
        lookAtAngle_Y = ClampAngle(lookAtAngle_Y, minClampRange, maxClampRange);

        transform.localEulerAngles = new Vector3(lookAtAngle_X, lookAtAngle_Y, 0.0f);
    }

    public void MoveForward()
    {
        CollisonCheckedMovement(CalculateForwardPosition());
    }

    public void MoveBackward()
    {
        CollisonCheckedMovement(-CalculateForwardPosition());
    }

    public void StrafeRight()
    {
        CollisonCheckedMovement(CalculateStrafePosition());
    }

    public void StrafeLeft()
    {
        CollisonCheckedMovement(-CalculateStrafePosition());
    }
    public void TurnRight()
    {
        TurnEulerDegrees(RightHand);
    }

    public void TurnLeft()
    {
        TurnEulerDegrees(LeftHand);
    }


    private void CollisonCheckedMovement(Vector3 movementDirection)
    {
        Vector3 targetPosition = transform.position + movementDirection;

        // TODO: Replace the true flag with your collision detection code!
        bool canMove = true;

        if (canMove)
        {
            movementTime = Time.deltaTime;
            moveFromPosition = transform.position;
            moveTowardsPosition = targetPosition;
        }
    }

    private void TurnEulerDegrees(in float eulerDirectionDelta)
    {
        rotationTime = Time.deltaTime;
        rotateFromDirection = transform.rotation;
        rotateTowardsDirection = rotateFromDirection;
        rotateTowardsDirection *= Quaternion.Euler(0.0f, eulerDirectionDelta, 0.0f);
    }

    public bool IsStationary()
    {
        return !(IsMoving() || IsRotating() || IsSnapBack() || freelookModeEnabled);
    }

    private bool IsMoving()
    {
        return movementTime > 0.0f;
    }

    private bool IsRotating()
    {
        return rotationTime > 0.0f;
    }

    private bool IsSnapBack()
    {
        return freelookTime > 0.0f;
    }

    private Vector3 CalculateForwardPosition()
    {
        return transform.forward * gridSize;
    }

    private Vector3 CalculateStrafePosition()
    {
        return transform.right * gridSize;
    }

    private void OnDisable()
    {
        ExitFreeLookMode();
    }

    private void EnterFreeLookMode()
    {
        freelookModeEnabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        freelookReturnToRotation = transform.localRotation;
    }

    private void ExitFreeLookMode()
    {
        freelookModeEnabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        freelookStartFromRotation = transform.localRotation;
        freelookTime = Time.deltaTime;
    }

    // Euler angles are modular and cant be simply clamped and therefore need special treatment.
    // I.E. 90° is the same as 450° or -270° and there is no gurantee which value you are getting
    // so we need to filter this out. This function is most likly suited to be placed some where else though
    // but the idea here is to have everything in a single simple script that you can attach to your camera.
    private float ClampAngle(float current, float min, float max)
    {
        float deltaAngle = Mathf.Abs(((min - max) + 180.0f) % 360.0f - 180.0f);
        float deltaAngleHalf = deltaAngle * 0.5f;
        float middelAngle = min + deltaAngleHalf;

        float offset = Mathf.Abs(Mathf.DeltaAngle(current, middelAngle)) - deltaAngleHalf;

        if (offset > 0.0f)
        {
            current = Mathf.MoveTowardsAngle(current, middelAngle, offset);
        }

        return current;
    }
}
