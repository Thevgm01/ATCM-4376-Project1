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
    public Transform _camera;

    public float speed = 6f;

    public float turnSmooth = 0.1f;
    private float turnSmoothVelocity;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        Idle?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        if(direction.sqrMagnitude >= 0.1f)
        {
            SetMoving(true);

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmooth);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 move = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _controller.Move(move * speed * Time.deltaTime);
        }
        else
        {
            SetMoving(false);
        }

        float jump = Input.GetAxisRaw("Jump");

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
