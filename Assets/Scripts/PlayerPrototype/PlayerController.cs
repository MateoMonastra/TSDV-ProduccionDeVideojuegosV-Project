using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody _rb;
    public CharacterController _controller;
    public Transform cam;


    private Vector3 direction;
    public float speed = 6f;
    public float dashSpeed = 6f;
    public float jumpSpeed = 6f;
    
    private bool grounded = false;
    private bool dashing = false;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.2f);
        if (hit.collider != null)
            grounded = true;
        else
            grounded = false;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        direction = new Vector3(horizontal, 0f, vertical);

        if (Input.GetButtonDown("Jump") && grounded)
        {
            _rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _rb.AddForce(transform.forward * dashSpeed);
        }
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

            float prevY = _rb.linearVelocity.y;
            _rb.linearVelocity = moveDir * (speed * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, prevY, _rb.linearVelocity.z);
        }
        else
        {
            //_rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, Vector3.down * 1.2f);
    }
}