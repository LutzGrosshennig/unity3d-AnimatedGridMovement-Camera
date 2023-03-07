/**************************************************************
 * The AnimatedGridMovement script performs "Dungeon Master"/ * 
 * "Legend of Grimrock" style WSADEQ movement in your Unity3D *
 * game.                                                      *
 *                                                            *
 * Written by: Lutz Grosshennig, 06/27/2020                   *
 * MIT Licence                                                *
 * ************************************************************/
using UnityEngine;

public class AnimatedGridMovement : MonoBehaviour
{
    private const float LeftHand = -90.0f;
    private const float RightHand = +90.0f;

    [Header("Grid settings")]
    [SerializeField] private float gridSize = 4.0f;

    [Header("Movement settings")]
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float movementSpeed = 1.0f;

    [Header("Free look settings")]
    [SerializeField] private float freelookSensitivity = 1.0f;
    [Range(45.0f, 89.0f)]
    [SerializeField] private float freelookAngle = 85.0f;


    private bool freelookModeEnabled = false;

    private Vector3 moveTowardsPosition;
    private Quaternion rotateFromDirection;
    private Quaternion rotateTowardsDirection;
  
    private float rotationTime = 0.0f;

    void Start()
    {
        moveTowardsPosition = transform.position;
        rotateTowardsDirection = transform.rotation;
        rotateFromDirection = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (IsStationary())
        {
            // Personaly I would prefer a dictionary<KeyCode,Action> lookup but this does not seem possible with Unity3D.Input
            if (Input.GetKey(KeyCode.W))
            {
                MoveForward();
            }
            else if (Input.GetKey(KeyCode.S))
            {
                MoveBackward();
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                TurnLeft();
            }
            else if (Input.GetKey(KeyCode.E))
            {
                TurnRight();
            }
            else if (Input.GetKey(KeyCode.A))
            {
                StrafeLeft();
            }
            else if (Input.GetKey(KeyCode.D))
            {
                StrafeRight();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            EnterFreeLookMode();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            ExitFreeLookMode();
        }

        if (freelookModeEnabled)
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
        else
        {
            if (IsMoving())
            {
                var step = Time.deltaTime * gridSize * movementSpeed;
                AnimateMovement(step);
            }
            if (IsRotating())
            {
                AnimateRotation();
            }
        }
    }

    private void AnimateRotation()
    {
        rotationTime += Time.deltaTime;
        transform.rotation = Quaternion.Slerp(rotateFromDirection, rotateTowardsDirection, rotationTime * rotationSpeed);
        CompensateRotationRoundingErrors();
    }

    private void AnimateMovement(float step)
    {
        transform.position = Vector3.MoveTowards(transform.position, moveTowardsPosition, step);
    }

    private void CompensateRotationRoundingErrors()
    {
        // Bear in mind that floating point numbers are inaccurate by design. 
        // The == operator performs a fuzy compare which means that we are only approximatly near the target rotation.
        // We may not entirely reached the rotateTowardsViewAngle or we may have slightly overshot it already (both within the margin of error).
        if (transform.rotation == rotateTowardsDirection)
        {
            // To compensate rounding errors we explictly set the transform to our desired rotation.
            transform.rotation = rotateTowardsDirection;
        }
    }

    private void MoveForward()
    {
        CollisonCheckedMovement(CalculateForwardPosition());
    }

    private void MoveBackward()
    {
        CollisonCheckedMovement(-CalculateForwardPosition());
    }

    private void StrafeRight()
    {
        CollisonCheckedMovement(CalculateStrafePosition());
    }

    private void StrafeLeft()
    {
        CollisonCheckedMovement(-CalculateStrafePosition());
    }

    private void CollisonCheckedMovement(Vector3 movementDirection)
    {
        Vector3 targetPosition = moveTowardsPosition + movementDirection;
        
        // TODO: Replace the true flag with your collision detection code.
        bool canMove = true;
        if (canMove)
        {
            moveTowardsPosition = targetPosition;
        }
    }

    private void TurnRight()
    {
        TurnEulerDegrees(RightHand);
    }

    private void TurnLeft()
    {
        TurnEulerDegrees(LeftHand);
    }

    private void TurnEulerDegrees(in float eulerDirectionDelta)
    {
        rotateFromDirection = transform.rotation;
        rotationTime = 0.0f;
        rotateTowardsDirection *= Quaternion.Euler(0.0f, eulerDirectionDelta, 0.0f);
    }

    private bool IsStationary()
    {
        return !(IsMoving() || IsRotating());
    }

    private bool IsMoving()
    {
        return transform.position != moveTowardsPosition;
    }

    private bool IsRotating()
    {
        return transform.rotation != rotateTowardsDirection;
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
    }

    private void ExitFreeLookMode()
    {
        freelookModeEnabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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
