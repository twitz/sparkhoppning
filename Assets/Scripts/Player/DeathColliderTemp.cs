using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class DeathColliderTemp : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;

    private void OnCollisionEnter(Collision other)
    {
        playerController.CollidedWith(other, gameObject);
    }
}
