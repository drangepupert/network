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
    public float mouseSensitivity = 2f;

    private float rotationX = 0f;

    private Camera playerCamera; // Reference to the player's camera
    private bool isCameraOwner;  // Flag to check camera ownership

    public override void OnNetworkSpawn()
    {
        randomnumber.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + "; randomnumber: " + randomnumber.Value);
        };

        characterController = GetComponent<CharacterController>();

        // Check if the camera has been instantiated
        if (playerCamera == null)
        {
            // Instantiate a new camera only for the local player
            if (IsOwner)
            {
                playerCamera = new GameObject("PlayerCamera").AddComponent<Camera>();
                playerCamera.transform.parent = transform;
                playerCamera.transform.localPosition = new Vector3(0f, 1.5f, 0f); // Adjust position based on your preference
                playerCamera.gameObject.AddComponent<AudioListener>(); // Optional: Add AudioListener for audio in the scene

                // Optionally, you can disable the original camera (if any) that might be in the prefab
                Camera[] cameras = GetComponentsInChildren<Camera>();
                foreach (Camera cam in cameras)
                {
                    if (cam != playerCamera)
                    {
                        cam.gameObject.SetActive(false);
                    }
                }

                isCameraOwner = true; // Set camera ownership flag
            }
        }

        // Other initialization logic...
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleMouseLook();
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
            MoveAccordingToCamera(moveSpeed);
        }
    }

    private void MoveAccordingToCamera(float moveSpeed)
    {
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal");

        if (moveDir != Vector3.zero)
        {
            characterController.Move(moveDir * moveSpeed * Time.deltaTime);
        }
    }

    private void Sprint(float moveSpeed)
    {
        float sprintMultiplier = 2f;

        // Use the player's forward and right directions to calculate the movement direction
        Vector3 moveDir = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")).normalized;

        // Apply sprinting speed
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

    #region MouseLook

    private void HandleMouseLook()
    {
        if (!isCameraOwner) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    #endregion
}