using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RippleController : MonoBehaviour
{

    public ParticleSystem ripple;
    public Rigidbody rb;
    public GameObject rippleCamera;

    // Update is called once per frame
    void Update()
    {
        rippleCamera.transform.position = transform.position + Vector3.up * 10;
    }

    private void createRipple(int Start, int End, int Delta, float Speed, float Size, float Lifetime) 
    {
        Vector3 forward = ripple.transform.forward;
        forward.y = Start;
        ripple.transform.eulerAngles = forward;

        for (int i = Start; i < End; i += Delta)
        {
            ripple.Emit(transform.position + ripple.transform.forward * 0.5f, ripple.transform.forward * Speed, Size, Lifetime, Color.white);
            ripple.transform.eulerAngles += Vector3.up * Delta;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.layer == 4 && rb.velocity.y > 0.02f)
        {
            createRipple(-180, 180, 3, 2, 2, 2);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 4 && Time.renderedFrameCount % 5 == 0 && Math.Sqrt(Math.Pow(rb.velocity.x, 2) + Math.Pow(rb.velocity.z, 2)) > 0.03f)
        {
            int y = (int) transform.eulerAngles.y;
            createRipple(y - 90, y + 90, 3, 5, 2, 1);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 4 && rb.velocity.y > 0.02f)
        {
            createRipple(-180, 180, 3, 2, 2, 2);
        }
    }
}
