using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    private NetworkVariable<int> randomnumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private CharacterController characterController;
    private float verticalSpeed = 0f;
    public float gravity = 9.81f;
    private float jumpForce = 8f;

    public override void OnNetworkSpawn()
    {
        randomnumber.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + "; randomnumber: " + randomnumber.Value);
        };

        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleMovementInput();
        HandleJumpInput();
        ApplyGravity();
    }

    #region Movement

    private void HandleMovementInput()
    {
        float moveSpeed = 3f;

    
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Sprint(moveSpeed);
        }
        else
        {
            MoveNormally(moveSpeed);
        }
    }

    private void MoveNormally(float moveSpeed)
    {
        Vector3 moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (moveDir != Vector3.zero)
        {
            characterController.Move(moveDir * moveSpeed * Time.deltaTime);
        }
    }

    private void Sprint(float moveSpeed)
    {
      
        float sprintMultiplier = 2f;

        Vector3 moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (moveDir != Vector3.zero)
        {
            characterController.Move(moveDir * moveSpeed * sprintMultiplier * Time.deltaTime);
        }
    }

    #endregion

    #region Jumping

    private void HandleJumpInput()
    {
        if (characterController.isGrounded)
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalSpeed = jumpForce;
            }
        }
    }

    #endregion

    #region Gravity

    private void ApplyGravity()
    {
       
        verticalSpeed -= gravity * Time.deltaTime;

      
        characterController.Move(new Vector3(0, verticalSpeed, 0) * Time.deltaTime);
    }

    #endregion
}