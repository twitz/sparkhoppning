using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private CharacterController characterController;

        [SerializeField]
        private Rigidbody ragdoll;

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
        private float steering = 5f;

        [SerializeField, Tooltip("Additional gravity for when the spark is in the air.")]
        private float jumpGravity = 1f;

        private new Transform transform;
        private Vector3 _movementVector;
        private bool canMove = true;

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
            // var movement = Physics.gravity;
            // if (_movementVector.sqrMagnitude > 0)
            // {
            //     movement += transform.forward + _movementVector * maxSpeed;
            // }
            // characterController.Move(movement * Time.deltaTime);

            if (!canMove) return;
            var movement = Physics.gravity;
            var localVelocity = characterController.velocity;
            if (_movementVector.z > 0)
            {
                var accelerationPerSecond = maxSpeed / acceleration;
                movement += transform.forward * Mathf.Min(localVelocity.z + accelerationPerSecond, maxSpeed);
            }
            else
            {
                var decelerationPerSecond = 0 / deceleration;
                movement -= transform.forward * Mathf.Max(0, localVelocity.z + decelerationPerSecond);
            }

            if (_movementVector.x != 0)
            {
                var accelerationPerSecond =
                    _movementVector.x * sideFriction; // TODO acceleration/smoothing (maxSpeed / acceleration)
                movement += new Vector3(localVelocity.x + accelerationPerSecond, 0, 0);
            }

            characterController.Move(movement * Time.deltaTime);
        }

        public void OnMovementInput(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            _movementVector = new Vector3(input.x, 0, input.y);
        }

        public void CollidedWith(Collision collider, GameObject initiator)
        {
            if (collider.gameObject.CompareTag("Obstacle"))
            {
                StartCoroutine(DeathSequence());
                Destroy(initiator);
            }
        }

        private IEnumerator DeathSequence()
        {
            canMove = false;
            characterController.enabled = false;
            ragdoll.transform.parent = null;
            ragdoll.isKinematic = false;
            ragdoll.AddForce(transform.forward * 10, ForceMode.Impulse);
            yield return new WaitForSeconds(3);
            SceneManager.LoadScene("Level");
        }
    }
}