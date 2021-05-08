using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private new Rigidbody rigidbody;

        [SerializeField]
        private Transform characterTransform;

        [SerializeField]
        private Rigidbody ragdoll;

        [Header("Cameras")]

        [SerializeField]
        private GameObject followCamera;
    
        [SerializeField]
        private GameObject speedCamera;

        [SerializeField]
        private GameObject deathCamera;
        
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
            followCamera.SetActive(true);
            speedCamera.SetActive(false);
            deathCamera.SetActive(false);
        }

        private void FixedUpdate()
        {
            HandleMovement();
            HandleCameras();
        }

        private void HandleMovement()
        {
            if (!canMove) return;
            var localVelocity = transform.InverseTransformVector(rigidbody.velocity);
            if (!(_movementVector.z > 0) || localVelocity.z >= maxSpeed) return;
            var force = _movementVector * acceleration;
            rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void LateUpdate()
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("PlayerSpeed", rigidbody.velocity.z);
            Debug.Log("Z Velocity " + rigidbody.velocity.z);
        }

        private void HandleCameras()
        {
            if (deathCamera.activeInHierarchy) return;
            var hasSpeed = rigidbody.velocity.sqrMagnitude > 0;
            switch (hasSpeed)
            {
                case true when !speedCamera.activeInHierarchy:
                    followCamera.SetActive(false);
                    speedCamera.SetActive(true);
                    break;
                case false when !followCamera.activeInHierarchy:
                    followCamera.SetActive(true);
                    speedCamera.SetActive(false);
                    break;
            }
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

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Obstacle"))
            {
                StartCoroutine(DeathSequence());
            }
        }

        private IEnumerator DeathSequence()
        {
            deathCamera.SetActive(true);
            followCamera.SetActive(false);
            speedCamera.SetActive(false);
            canMove = false;
            ragdoll.transform.parent = null;
            ragdoll.isKinematic = false;
            ragdoll.AddForce(transform.forward * 10, ForceMode.Impulse);
            yield return new WaitForSeconds(3);
            SceneManager.LoadScene("Level");
        }
    }
}