using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private CharacterController characterController;

        [Header("Movement Settings")]
        [SerializeField, Min(0.001f)]
        private float baseMovementSpeed;

        [SerializeField, Min(0.001f)]
        private float maxSpeed = 10f;

        [SerializeField]
        private float acceleration = 5f;

        [SerializeField, Tooltip("How quickly the spark slows down while braking")]
        private float brakeSpeed = 10f;

        [SerializeField, Tooltip("How quickly the spark slows down when no input is detected")]
        private float deceleration = 4f;

        [SerializeField, Range(0f, 1f)]
        private float sideFriction = 0.95f;

        [SerializeField, Tooltip("How tightly the kart can turn left or right.")]
        public float steering = 5f;

        [SerializeField, Tooltip("Additional gravity for when the kart is in the air.")]
        public float jumpGravity = 1f;

        private new Transform transform;
        private Vector3 _movementVector;

        private void Start()
        {
            transform = base.transform;
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            var movementVector = transform.forward + Physics.gravity + _movementVector * baseMovementSpeed;
            characterController.Move(movementVector * Time.deltaTime);
        }

        public void OnMovementInput(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            _movementVector = new Vector3(input.x, 0, input.y);
        }
    }
}