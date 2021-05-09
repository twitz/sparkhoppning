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

        [SerializeField]
        private GameObject victoryCamera;

        [Header("Movement Settings")]
        [SerializeField, Min(0.001f)]
        private float maxSpeed = 30f;

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

        [SerializeField, FMODUnity.EventRef]
        private string playerDeathEvent;

        private new Transform transform;
        private Vector3 _movementVector;
        private bool _canMove = true;
        private int klubbadakCollisionCount = 0;
        private float snowBuildup = 0f; //Subtraheras från maxSpeed, går från 0 till 15
        private int powderCollisionCount = 0;
        private float powderBuildup = 0f; //Subtraheras från maxSpeed, går från 0 till 10

        private bool _isVictorySequenceActive;
        private bool _hasLanded;
        private int _distanceJumped;

        private void Start()
        {
            transform = base.transform;
            followCamera.SetActive(true);
            speedCamera.SetActive(false);
            deathCamera.SetActive(false);
            victoryCamera.SetActive(false);
        }

        private void FixedUpdate()
        {
            HandleMovement();
            HandleCameras();
        }

        private void HandleMovement()
        {
            if (!_canMove) return;
            var localVelocity = transform.InverseTransformVector(rigidbody.velocity);
            //if (!(_movementVector.z > 0) || localVelocity.z >= maxSpeed) return;

            //Klubbadak-kod
            if (klubbadakCollisionCount > 0)
            {
                snowBuildup += 30f * Time.fixedDeltaTime;
                if (snowBuildup > 15f)
                {
                    snowBuildup = 15f;
                }
            }
            else
            {
                snowBuildup -= 6.25f * Time.fixedDeltaTime;
                if (snowBuildup < 0f)
                {
                    snowBuildup = 0f;
                }
            }

            //Puder-kod
            if (powderCollisionCount > 0)
            {
                powderBuildup += 30f * Time.fixedDeltaTime;
                if (powderBuildup > 10f)
                {
                    powderBuildup = 10f;
                }
            }
            else
            {
                powderBuildup -= 6.25f * Time.fixedDeltaTime;
                if (powderBuildup < 0f)
                {
                    powderBuildup = 0f;
                }
            }

            var force = _movementVector * acceleration;
            if (localVelocity.z >= (maxSpeed - snowBuildup + powderBuildup))
            {
                force.z = -200f * Time.fixedDeltaTime;
            }

            // Det krävs lite fart för att svänga
            if (localVelocity.z < 0.1f)
            {
                force.x = 0f;
            }
            else
            {
                var turnModifier = (localVelocity.z + 4f) / maxSpeed;
                if (turnModifier > 1f)
                {
                    turnModifier = 1f;
                }
                else if (turnModifier < 0.3f)
                {
                    turnModifier = 0.3f;
                }

                force.x *= turnModifier;
            }

            rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void LateUpdate()
        {
            Debug.Log("X Velocity " + rigidbody.velocity.x);
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("PlayerSpeed", rigidbody.velocity.z);
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("PlayerTurn", rigidbody.velocity.x);
        }

        private void HandleCameras()
        {
            if (deathCamera.activeInHierarchy || _isVictorySequenceActive) return;
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

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Obstacle"))
            {
                StartCoroutine(DeathSequence());
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            switch (other.gameObject.tag)
            {
                case "Klubbadak":
                    klubbadakCollisionCount += 1;
                    break;
                case "OutOfBounds" when !_isVictorySequenceActive:
                    StartCoroutine(DeathSequence());
                    break;
                case "Finish":
                    StartCoroutine(VictorySequence());
                    break;
                case "OutOfBounds" when _isVictorySequenceActive:
                    _hasLanded = true;
                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Klubbadak"))
            {
                klubbadakCollisionCount -= 1;
            }
        }

        private IEnumerator DeathSequence()
        {
            if (playerDeathEvent != null)
            {
                FMODUnity.RuntimeManager.PlayOneShot(playerDeathEvent, transform.position);
            }

            deathCamera.SetActive(true);
            followCamera.SetActive(false);
            speedCamera.SetActive(false);
            _canMove = false;
            ragdoll.transform.parent = null;
            ragdoll.isKinematic = false;
            ragdoll.AddForce(transform.forward * 20, ForceMode.Impulse);
            yield return Restart();
        }

        private IEnumerator VictorySequence()
        {
            _isVictorySequenceActive = true;
            _canMove = false;
            followCamera.SetActive(false);
            speedCamera.SetActive(false);
            victoryCamera.SetActive(true);
            var startPosition = transform.position;
            while (!_hasLanded)
            {
                Debug.Log(transform.position - startPosition);
                yield return null;
            }
        }

        private IEnumerator Restart()
        {
            yield return new WaitForSeconds(3);
            SceneManager.LoadScene("Level");
        }
    }
}