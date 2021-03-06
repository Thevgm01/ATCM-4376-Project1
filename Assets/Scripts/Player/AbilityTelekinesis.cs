﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityTelekinesis : MonoBehaviour, IAbility
{
    public Transform _camera;

    public float telekinesisDistance = 10;
    public float telekinesisLaunchForce = 50;

    //private GameObject aimTarget;
    private bool casting;
    private bool grabbed;

    private Rigidbody telekinesisRB;
    private Transform telekinesisDestination;

    LayerMask grabbableObjectsMask;

    public ParticleSystem telekinesisDestinationParticles;
    public ParticleSystem telekinesisObjectParticles;

    public AudioClip startTelekinesisSound;
    public AudioClip holdTelekinesisSound;
    AudioSource holdTelekinesisSource;
    public AudioClip stopTelekinesisSound;
    public AudioClip[] impactSounds;

    void Awake()
    {
        telekinesisDestination = transform.Find("Telekinesis Point");

        grabbableObjectsMask = LayerMask.GetMask("Grabbable");

        telekinesisDestinationParticles.Stop();
    }

    void FixedUpdate()
    {
        if (casting)
        {
            if (!grabbed) TryGrab();
            else if(telekinesisRB != null)
            {
                telekinesisRB.transform.position = Vector3.Lerp(telekinesisRB.transform.position, telekinesisDestination.position, 0.15f / telekinesisRB.mass);
                telekinesisRB.velocity = Vector3.zero;
                return;
            }
        }

        Physics.Raycast(_camera.position, _camera.forward, out var ray, telekinesisDistance, grabbableObjectsMask);
        if (ray.collider != null && ray.collider.tag != "Ground") // If the ray hit something that's not the ground
        {
            if(telekinesisRB == null || telekinesisRB.gameObject != ray.collider.gameObject) // Switching to a new object
            {
                SetOutline(false);
                telekinesisRB = ray.collider.GetComponent<Rigidbody>(); // Get the RB of the new object
                SetOutline(true);
            }
        }
        else if(telekinesisRB != null) // If the ray didn't hit something
        {
            telekinesisRB.GetComponent<Outline>().enabled = false;
            telekinesisRB = null;
        }
    }

    void SetOutline(bool val) { SetOutline(telekinesisRB, val); }
    void SetOutline(Rigidbody rb, bool val)
    {
        if (rb != null)
        {
            var outline = rb.GetComponent<Outline>(); // Does it already have an outline?
            if (outline == null) // If not, add one
            {
                outline = rb.gameObject.AddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.OutlineWidth = 5f;
            }
            outline.enabled = val;
        }
    }

    void TryGrab()
    {
        if (telekinesisRB != null)
        {
            grabbed = true;

            telekinesisRB.useGravity = false;
            telekinesisRB.angularVelocity = UnityEngine.Random.insideUnitSphere * 10;

            telekinesisDestinationParticles.Play();
            Instantiate(telekinesisObjectParticles, telekinesisRB.transform);

            AudioHelper.PlayClip2D(startTelekinesisSound, 1f);
            holdTelekinesisSource = AudioHelper.PlayClip2D(holdTelekinesisSound, 0f, 1f / Mathf.Sqrt(telekinesisRB.mass), false);
            holdTelekinesisSource.loop = true;
            StartCoroutine("RaiseHoldVolume");
        }
    }

    IEnumerator RaiseHoldVolume()
    {
        while(holdTelekinesisSource != null && holdTelekinesisSource.volume < 0.25f)
        {
            holdTelekinesisSource.volume += 0.0005f;
            yield return null;
        }
    }

    public void StartCast()
    {
        casting = true;
        TryGrab();
    }

    public void FinishCast()
    {
        casting = false;
        grabbed = false;

        if (telekinesisRB != null)
        {
            telekinesisRB.useGravity = true;
            telekinesisRB.velocity = _camera.forward * telekinesisLaunchForce / Mathf.Sqrt(telekinesisRB.mass);
            StartCoroutine("ChangeRigidbodyCollisionMode", telekinesisRB);
            SetOutline(false);
            telekinesisRB = null;

            telekinesisDestinationParticles.Stop();

            AudioHelper.PlayClip2D(stopTelekinesisSound, 1f);
            Destroy(holdTelekinesisSource.gameObject);
        }
    }

    IEnumerator ChangeRigidbodyCollisionMode(Rigidbody rb)
    {
        float originalVelocity = rb.velocity.magnitude;
        float velocityThreshold = originalVelocity * originalVelocity * 0.5f;

        CollisionDetectionMode originalMode = rb.collisionDetectionMode;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        while(rb.velocity.sqrMagnitude > velocityThreshold)
        {
            yield return new WaitForSeconds(0.1f);
        }

        rb.collisionDetectionMode = originalMode;

        var particles = rb.transform.GetComponentInChildren<ParticleSystem>();
        var main = particles.main;
        particles.Stop();
        Destroy(particles.gameObject, main.duration);

        CameraShaker.Shake();

        AudioHelper.PlayRandomClip2DFromArray(impactSounds, 1f, 1f / Mathf.Sqrt(rb.mass));
    }

    void OnDisable()
    {
        FinishCast();
    }
}
