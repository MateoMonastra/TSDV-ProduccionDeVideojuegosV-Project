using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody _rb;
    public CharacterController _controller;
    public Transform cam;


    private Vector3 direction;
    public float speed = 6f;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        direction = new Vector3(horizontal, 0f, vertical);
    }

    private void FixedUpdate()
    {
        if (direction.magnitude >= 0.01f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _rb.AddForce(moveDir.normalized * speed * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
        else
        {
            Debug.Log("problema");
            _rb.linearVelocity = Vector3.zero;
        }
    }
}