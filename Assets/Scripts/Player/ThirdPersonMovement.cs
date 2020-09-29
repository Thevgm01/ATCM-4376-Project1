using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    bool _isMoving = false;

    FootTracker _feet;

    CharacterController _controller;
    Animator _animator;
    public Transform _camera;

    public float speed = 6f;
    public float jumpHeight = 5f;

    public float sprintSpeedMult = 1.5f;
    private float sprintTime = 0;
    private float sprintSpeedUp = 1f;

    public float turnSmooth = 0.1f;
    private float turnSmoothVelocity;
    private float currentTelekinesisAngle = 0;
    private float telekinesisTurnSmoothVelocity;

    private bool usingTelekinesis = false;
    AbilityTelekinesis _telekinesis;
    public ParticleSystem telekinesisFlyParticles;
    AudioSource flyTelekinesisSource;

    Health _health;

    private Vector3 velocity;
    private bool flying;
    private bool beganFlying;

    public AudioClip[] jumpSounds;
    public ParticleSystem landParticles;

    public AudioClip[] footstepSounds;
    public float distancePerFootstep;
    float curFootstepDistance;
    public ParticleSystem runningParticles;

    public AudioClip[] hurtSounds;
    public AudioClip[] deathSounds;

    void Awake()
    {
        _feet = GetComponentInChildren<FootTracker>();
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _telekinesis = GetComponent<AbilityTelekinesis>();
        _health = GetComponent<Health>();

        _health.onDamaged += Damaged;
        _health.onKilled += Died;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_health.alive)
        {
            HandleTelekinesis();
            HandleMovement();
        }
        HandleGravity();
    }

    void HandleTelekinesis()
    {
        if(!usingTelekinesis)
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (!beganFlying)
                {
                    beganFlying = true;
                    _animator.SetTrigger("Flying");

                    telekinesisFlyParticles.Play();
                    AudioHelper.PlayClip2D(_telekinesis.startTelekinesisSound, 1f);
                    flyTelekinesisSource = AudioHelper.PlayClip2D(_telekinesis.holdTelekinesisSound, 0f, 1f, false);
                    flyTelekinesisSource.loop = true;
                    StartCoroutine("RaiseFlyVolume");
                }
                flying = true;

            }
            else if (Input.GetMouseButtonUp(1))
            {
                flying = false;
            }
        }
        
        if(!flying)
        {
            if (Input.GetMouseButtonDown(0))
            {
                usingTelekinesis = true;
                _telekinesis.StartCast();
                _animator.SetTrigger("Telekinesis On");
            }
            else if (Input.GetMouseButtonUp(0))
            {
                usingTelekinesis = false;
                _telekinesis.FinishCast();
                _animator.SetTrigger("Telekinesis Off");
            }
        }

        _animator.SetBool("Telekinesis", usingTelekinesis || flying);
    }

    IEnumerator RaiseFlyVolume()
    {
        while(flyTelekinesisSource != null && flyTelekinesisSource.volume < 0.25f)
        {
            flyTelekinesisSource.volume += 0.0005f;
            yield return null;
        }
    }


    void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        _animator.SetFloat("Horizontal Input", horizontalInput);
        _animator.SetFloat("Vertical Input", verticalInput);

        Vector3 direction = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        Vector3 move = Vector3.zero;
        if (direction.magnitude >= 0.1f)
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
                    if (sprintTime < 1) sprintTime += sprintSpeedUp * Time.deltaTime;
                    if (!runningParticles.isPlaying) runningParticles.Play();
                    _animator.SetBool("Sprint", true);
                }

                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, desiredFaceAngle, ref turnSmoothVelocity, turnSmooth);
            }

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            move = Quaternion.Euler(0f, desiredFaceAngle, 0f) * Vector3.forward;
        }

        if (direction.magnitude < 0.1f)
        {
            sprintTime = 0f;
            if (runningParticles.isPlaying) runningParticles.Stop();
            _animator.SetBool("Sprint", false);
        }
        else if (sprintTime > 0 && (usingTelekinesis || !Input.GetKey(KeyCode.LeftShift)))
        {
            sprintTime -= sprintSpeedUp * Time.deltaTime;
            if (runningParticles.isPlaying) runningParticles.Stop();
            _animator.SetBool("Sprint", false);
        }

        Vector3 lateral = move * speed * Mathf.Lerp(1, sprintSpeedMult, sprintTime);
        Vector3 vertical = new Vector3(0, velocity.y, 0);
        _controller.Move((lateral + vertical) * Time.deltaTime);

        if (_feet.grounded)
        {
            curFootstepDistance += lateral.magnitude * Time.deltaTime;
            if (curFootstepDistance >= distancePerFootstep)
            {
                curFootstepDistance -= distancePerFootstep;
                AudioHelper.PlayRandomClip2DFromArray(footstepSounds, 1f);
            }
        }

        _animator.SetFloat("Speed", lateral.magnitude);
    }

    void HandleGravity()
    {
        float jumpInput = Input.GetAxisRaw("Jump");
        if (_feet.grounded)
        {
            if (_health.alive && jumpInput >= 0.1f && velocity.y <= 0f)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
                _animator.SetTrigger("Jump");
                _animator.SetBool("Landed", false);
                Instantiate(landParticles, _feet.transform.position, Quaternion.identity);
                AudioHelper.PlayRandomClip2DFromArray(jumpSounds, 0.5f);
                AudioHelper.PlayRandomClip2DFromArray(footstepSounds, 1f, 0.3f);
            }
            else if (velocity.y < 0f)
            {
                if(velocity.y < -1f)
                {
                    Instantiate(landParticles, _feet.transform.position, Quaternion.identity);
                    AudioHelper.PlayRandomClip2DFromArray(jumpSounds, 0.5f);
                    AudioHelper.PlayRandomClip2DFromArray(footstepSounds, 1f, 0.3f);
                    if (beganFlying)
                    {
                        _animator.ResetTrigger("Flying");
                        telekinesisFlyParticles.Stop();
                        Destroy(flyTelekinesisSource.gameObject);
                        beganFlying = false;
                    }
                }
                _animator.ResetTrigger("Jump");
                _animator.SetBool("Landed", true);
                velocity.y = 0;
            }
        }
        if (flying)
        {
            velocity.y -= Physics.gravity.y * Time.deltaTime;
            _animator.SetBool("Landed", false);
        }
        else velocity.y += Physics.gravity.y * Time.deltaTime;
        _animator.SetFloat("Vertical Speed", velocity.y);
    }

    void Damaged(int newHP)
    {
        CameraShaker.Shake();

        AudioHelper.PlayRandomClip2DFromArray(hurtSounds, 1f);
    }

    void Died()
    {
        _telekinesis.enabled = false;

        int numDeathAnimations = speed > 0.1f ? 6 : 5;

        _animator.SetFloat("Death Animation", UnityEngine.Random.Range(0, numDeathAnimations));
        _animator.SetTrigger("Die");

        CameraShaker.Shake();

        AudioHelper.PlayRandomClip2DFromArray(deathSounds, 1f);
    }
}
