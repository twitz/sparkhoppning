using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuitGameListener : MonoBehaviour
{
    public void OnEscapeInput(InputAction.CallbackContext context)
    {
        Application.Quit();
    }
}
