using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    public event Action Idle = delegate { };
    public event Action StartRunning = delegate { };

    bool _isMoving = false;

    CharacterController _controller;
    Animator _animator;
    public Transform _camera;

    public float speed = 6f;
    public float jumpHeight = 5f;
    private float y_vel = 0f;

    public float turnSmooth = 0.1f;
    private float turnSmoothVelocity;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        Idle?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        float playerHorizontal = Input.GetAxisRaw("Horizontal");
        float playerVertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(playerHorizontal, 0f, playerVertical).normalized;
        Vector3 move = Vector3.zero;
        if(direction.sqrMagnitude >= 0.1f)
        {

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmooth);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            move = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        _animator.SetFloat("Speed", direction.magnitude);

        float jump = Input.GetAxisRaw("Jump");
        if(_controller.isGrounded)
        {
            if (jump >= 0.1f)
            {
                y_vel = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
                _animator.SetBool("Jump", true);
            }
            else if(y_vel != 0f)
            {
                y_vel = 0f;
                _animator.SetBool("Jump", false);
            }
        }
        else
        {
            y_vel += Physics.gravity.y * Time.deltaTime;
            _animator.SetBool("Jump", false);
        }

        Vector3 lateral = move * speed;
        Vector3 vertical = new Vector3(0, y_vel, 0);

        _controller.Move((lateral + vertical) * Time.deltaTime);
    }

    void SetMoving(bool move)
    {
        if(move && !_isMoving)
        {
            StartRunning?.Invoke();
            _isMoving = true;
        }
        else if(!move && _isMoving)
        {
            Idle?.Invoke();
            _isMoving = false;
        }
    }
}
