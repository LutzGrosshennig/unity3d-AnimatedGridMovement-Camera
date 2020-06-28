/**************************************************************
 * The AnimatedGridMovement script performs "Dungeon Master"/ * 
 * "Legend of Grimrock" style WSAEQ movement in your Unity3D  *
 * game.                                                      *
 *                                                            *
 * Written by: Lutz Grosshennig, 06/27/2020                   *
 * MIT Licence                                                *
 * ************************************************************/
using UnityEngine;

public class AnimatedGridMovement : MonoBehaviour
{
    public float gridSize = 4.0f;
    public float rotationMulitplier = 5.0f;

    private const float LeftHand = -90.0f;
    private const float RightHand = +90.0f;

    private Vector3 moveTowardsPosition;
    private Quaternion rotateTowardsDirection;

    void Start()
    {
        moveTowardsPosition = transform.position;
        rotateTowardsDirection = transform.rotation;
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
        var step = Time.deltaTime * gridSize;

        if (IsMoving())
        {
            AnimateMovement(step);
        }
        if (IsRotating())
        {
            AnimateRotation(step);
        }
    }

    private void AnimateRotation(float step)
    {
        transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, rotateTowardsDirection, step * rotationMulitplier);
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
        moveTowardsPosition += CalculateOneGridPositionForward();
    }

    private void MoveBackward()
    {
        moveTowardsPosition -= CalculateOneGridPositionForward();
    }

    private void StrafeRight()
    {
        StrafeOneGridPosition(RightHand);
    }

    private void StrafeLeft()
    {
        StrafeOneGridPosition(LeftHand);
    }

    private void TurnRight()
    {
        TurnCamera(RightHand);
    }

    private void TurnLeft()
    {
        TurnCamera(LeftHand);
    }

    private bool IsStationary()
    {
        return !IsMoving() && !IsRotating();
    }

    private bool IsMoving()
    {
        return !(transform.position == moveTowardsPosition);
    }

    private bool IsRotating()
    {
        return !(transform.rotation == rotateTowardsDirection);
    }

    private Vector3 CalculateOneGridPositionForward()
    {
        return transform.forward * gridSize;
    }

    private void TurnCamera(in float angle)
    {
        RotateCameraTemporaryAndPerformAction(angle, () => { rotateTowardsDirection = transform.rotation; });
    }

    private void StrafeOneGridPosition(in float angle)
    {
        RotateCameraTemporaryAndPerformAction(angle, () => { moveTowardsPosition += CalculateOneGridPositionForward(); });
    }

    private void RotateCameraTemporaryAndPerformAction(in float angle, System.Action lambdaFunction)
    {
        var rotationTemp = transform.rotation;
        transform.Rotate(Vector3.up, angle);
        lambdaFunction();
        transform.rotation = rotationTemp;
    }
}
