using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootTracker : MonoBehaviour
{
    int numContacted = 0;
    public bool grounded
    {
        get => numContacted > 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player")
        {
            numContacted++;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Player")
        {
            numContacted--;
        }
    }

}
