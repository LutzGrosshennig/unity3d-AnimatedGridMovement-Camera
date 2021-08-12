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

    [SerializeField] private float gridSize = 4.0f;
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float movementSpeed = 1.0f;

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
}
