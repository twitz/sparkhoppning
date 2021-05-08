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
        private float maxSpeed = 10f;

        [SerializeField]
        private float acceleration = 5f;

        [SerializeField, Tooltip("How quickly the spark slows down while braking")]
        private float brakeSpeed = 4f;

        [SerializeField, Tooltip("How quickly the spark slows down when no input is detected")]
        private float deceleration = 10f;

        [SerializeField, Range(0f, 1f)]
        private float sideFriction = 0.95f;

        [SerializeField, Tooltip("How tightly the spark can turn left or right.")]
        public float steering = 5f;

        [SerializeField, Tooltip("Additional gravity for when the spark is in the air.")]
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
            var movement = Physics.gravity;
            if (_movementVector.sqrMagnitude > 0)
            {
                movement += transform.forward + _movementVector * maxSpeed;
            }
            characterController.Move(movement * Time.deltaTime);
            
            // var movement = Physics.gravity;
            // var localVelocity = characterController.velocity;
            // if (_movementVector.z > 0)
            // {
            //     var accelerationPerSecond = maxSpeed / acceleration;
            //     movement += transform.forward * Mathf.Min(localVelocity.z + accelerationPerSecond, maxSpeed);
            // }
            // else
            // {
            //     var decelerationPerSecond = 0 / deceleration;
            //     movement -= transform.forward * Mathf.Max(0, localVelocity.z + decelerationPerSecond);
            // }
            //
            // if (_movementVector.x != 0)
            // {
            //     var accelerationPerSecond = _movementVector.x * (maxSpeed / acceleration) * sideFriction;
            //     movement += new Vector3(localVelocity.x + accelerationPerSecond, 0, 0);
            // }
            //
            // characterController.Move(movement * Time.deltaTime);
        }

        private void LateUpdate()
        {
            // Debug.Log("Velocity " + transform.InverseTransformDirection(characterController.velocity));
        }

        public void OnMovementInput(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            _movementVector = new Vector3(input.x, 0, input.y);
        }
    }
}