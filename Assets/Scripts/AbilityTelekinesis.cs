using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityTelekinesis : MonoBehaviour, Ability
{
    public Transform _camera;

    public float telekinesisLaunchForce = 1000;

    private GameObject aimTarget;
    private bool casting;

    private GameObject telekinesisTarget;
    private Rigidbody telekinesisRB;
    private Transform telekinesisDestination;
    public ParticleSystem telekinesisParticles;
    public AudioClip startTelekinesisSound;
    public AudioClip stopTelekinesisSound;

    void Awake()
    {
        telekinesisDestination = transform.Find("Telekinesis Point");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (casting)
        {
            if (telekinesisRB == null) TryGrab();
            else telekinesisRB.transform.position = Vector3.Lerp(telekinesisTarget.transform.position, telekinesisDestination.position, 0.15f / telekinesisRB.mass);
            return;
        }

        Physics.Raycast(_camera.position, _camera.forward, out var ray);
        if (ray.collider != null && ray.collider.tag != "Ground")
        {
            if(aimTarget != ray.collider.gameObject)
            {
                if (aimTarget != null)
                {
                    aimTarget.GetComponent<Outline>().enabled = false;
                }
                aimTarget = ray.collider.gameObject;
                var telekinesisOutline = aimTarget.GetComponent<Outline>();
                if (telekinesisOutline == null)
                {
                    telekinesisOutline = ray.collider.gameObject.AddComponent<Outline>();
                    telekinesisOutline.OutlineMode = Outline.Mode.OutlineVisible;
                    telekinesisOutline.OutlineWidth = 5f;
                }
                else
                {
                    telekinesisOutline.enabled = true;
                }
            }
        }
        else if(aimTarget != null)
        {
            aimTarget.GetComponent<Outline>().enabled = false;
            aimTarget = null;
        }
    }

    void TryGrab()
    {
        if (aimTarget != null)
        {
            telekinesisRB = aimTarget.GetComponent<Rigidbody>();
            if (telekinesisRB != null)
            {
                telekinesisRB.useGravity = false;
                telekinesisRB.angularVelocity = UnityEngine.Random.insideUnitSphere;
                //telekinesisParticles.Play();
                AudioHelper.PlayClip2D(startTelekinesisSound, 1f);
            }
        }
    }

    public void StartCast()
    {
        casting = true;
    }

    public void FinishCast()
    {
        if (telekinesisRB != null)
        {
            telekinesisRB.useGravity = true;
            telekinesisRB.velocity = _camera.forward * telekinesisLaunchForce;// / telekinesisRB.mass;
            telekinesisRB = null;
            telekinesisParticles.Stop();
            AudioHelper.PlayClip2D(stopTelekinesisSound, 1f);
        }
    }
}
