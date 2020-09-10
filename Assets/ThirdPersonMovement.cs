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

    public float sprintSpeedMult = 1.5f;
    private float sprintTime = 0;
    private float sprintSpeedUp = 0.01f;

    public float turnSmooth = 0.1f;
    private float turnSmoothVelocity;
    private float telekinesisAngle = 0;
    private float telekinesisTurnSmoothVelocity;

    private bool usingTelekinesis = false;

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
        if(Input.GetMouseButtonDown(0))
        {
            usingTelekinesis = true;
            _animator.SetTrigger("Telekinesis On");
        }
        else if(Input.GetMouseButtonUp(0))
        {
            usingTelekinesis = false;
            _animator.SetTrigger("Telekinesis Off");
        }
        _animator.SetBool("Telekinesis", usingTelekinesis);

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        _animator.SetFloat("Horizontal Input", horizontalInput);
        _animator.SetFloat("Vertical Input", verticalInput);

        Vector3 direction = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        Vector3 move = Vector3.zero;
        if(direction.magnitude >= 0.1f)
        {

            float targetMoveAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;

            float angle;
            if (usingTelekinesis)
            {
                float targetFaceAngle = 0f;
                if ((horizontalInput > 0.1f && verticalInput > 0.1f) ||
                    (horizontalInput < -0.1f && verticalInput < -0.1f))
                    targetFaceAngle = 45f;
                else if
                   ((horizontalInput > 0.1f && verticalInput < -0.1f) ||
                    (horizontalInput < -0.1f && verticalInput > 0.1f))
                    targetFaceAngle = -45f;
                //angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _camera.eulerAngles.y + targetFaceAngle, ref turnSmoothVelocity, turnSmooth);
                float targetTelekinesisAngle = Mathf.Atan2(direction.x, direction.z) / Mathf.PI;
                if (telekinesisAngle < 0 && targetTelekinesisAngle == 1) targetTelekinesisAngle = -1;
                telekinesisAngle = Mathf.SmoothDampAngle(telekinesisAngle, targetTelekinesisAngle, ref telekinesisTurnSmoothVelocity, turnSmooth);
                _animator.SetFloat("Direction", telekinesisAngle);

                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _camera.eulerAngles.y, ref turnSmoothVelocity, turnSmooth);
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (sprintTime < 1) sprintTime += sprintSpeedUp;
                    _animator.SetBool("Sprint", true);
                }

                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetMoveAngle, ref turnSmoothVelocity, turnSmooth);
            }
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            move = Quaternion.Euler(0f, targetMoveAngle, 0f) * Vector3.forward;
        }

        if (sprintTime > 0 && (usingTelekinesis || !Input.GetKey(KeyCode.LeftShift)))
        {
            sprintTime -= sprintSpeedUp;
            _animator.SetBool("Sprint", false);
        }

        _animator.SetFloat("Speed", direction.magnitude);

        float jump = Input.GetAxisRaw("Jump");
        if(_controller.isGrounded)
        {
            if (jump >= 0.1f)
            {
                y_vel = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
                _animator.SetTrigger("Jump");
                _animator.SetBool("Landed", false);
            }
            else if(y_vel != 0f)
            {
                y_vel = 0f;
                _animator.SetBool("Landed", true);
            }
        }
        else
        {
            y_vel += Physics.gravity.y * Time.deltaTime;
        }

        Vector3 lateral = move * speed * Mathf.Lerp(1, sprintSpeedMult, sprintTime);
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
