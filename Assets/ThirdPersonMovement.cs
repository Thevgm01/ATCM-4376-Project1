using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
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
    private float currentTelekinesisAngle = 0;
    private float telekinesisTurnSmoothVelocity;

    public float telekinesisLaunchForce = 1000;
    private bool usingTelekinesis = false;
    private GameObject telekinesisTarget;
    private Rigidbody telekinesisRB;
    private Transform telekinesisGoal;
    public ParticleSystem telekinesisParticles;
    public AudioClip startTelekinesisSound;
    public AudioClip stopTelekinesisSound;

    private AudioSource audioSource;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        telekinesisGoal = transform.Find("Telekinesis Point");
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
            ReleaseTelekinesis();
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
            float moveAngle = Mathf.Atan2(direction.x, direction.z);
            float desiredFaceAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;

            float angle;
            if (usingTelekinesis)
            {
                float desiredTelekinesisAngle = moveAngle / Mathf.PI;
                if (currentTelekinesisAngle < 0 && desiredTelekinesisAngle == 1) desiredTelekinesisAngle = -1;
                currentTelekinesisAngle = Mathf.SmoothDampAngle(currentTelekinesisAngle, desiredTelekinesisAngle, ref telekinesisTurnSmoothVelocity, turnSmooth);
                _animator.SetFloat("Direction", currentTelekinesisAngle);

                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _camera.eulerAngles.y, ref turnSmoothVelocity, turnSmooth);
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (sprintTime < 1) sprintTime += sprintSpeedUp;
                    _animator.SetBool("Sprint", true);
                }

                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, desiredFaceAngle, ref turnSmoothVelocity, turnSmooth);
            }
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            move = Quaternion.Euler(0f, desiredFaceAngle, 0f) * Vector3.forward;
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
        _animator.SetFloat("Vertical Speed", y_vel);

        Vector3 lateral = move * speed * Mathf.Lerp(1, sprintSpeedMult, sprintTime);
        Vector3 vertical = new Vector3(0, y_vel, 0);

        _controller.Move((lateral + vertical) * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (usingTelekinesis)
        {
            HandleTelekinesis();
        }
    }

    void HandleTelekinesis()
    {
        if(telekinesisTarget == null)
        {
            Physics.Raycast(_camera.position, _camera.forward, out var ray);
            if(ray.collider != null && ray.collider.tag != "Ground")
            {
                telekinesisRB = ray.collider.GetComponent<Rigidbody>();
                if(telekinesisRB != null)
                {
                    telekinesisTarget = ray.collider.gameObject;
                    telekinesisRB.useGravity = false;
                    telekinesisRB.angularVelocity = UnityEngine.Random.insideUnitSphere;
                    telekinesisParticles.Play();
                    audioSource.PlayOneShot(startTelekinesisSound);
                }
            }
        }

        if(telekinesisTarget != null)
        {
            telekinesisTarget.transform.position = Vector3.Lerp(telekinesisTarget.transform.position, telekinesisGoal.position, 0.15f / telekinesisRB.mass);
        }
    }

    void ReleaseTelekinesis()
    {
        if(telekinesisTarget != null)
        {
            telekinesisRB.useGravity = true;
            telekinesisRB.velocity = _camera.forward * telekinesisLaunchForce / telekinesisRB.mass;
            telekinesisTarget = null;
            telekinesisRB = null;
            telekinesisParticles.Stop();
            audioSource.PlayOneShot(stopTelekinesisSound);
        }
    }
}
