using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float Speed = .2f;
    public float Gravity = 10f;
    private void FixedUpdate()
    {
        var controller = GetComponent<CharacterController>();
        var v = Input.GetAxis("Vertical");
        var h = Input.GetAxis("Horizontal");
        var walking = Speed * (transform.forward * v + transform.right * h).normalized;
        controller.Move(Time.fixedDeltaTime * (walking + Vector3.down * Gravity));
    }
}
