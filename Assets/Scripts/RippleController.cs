using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RippleController : MonoBehaviour
{

    public ParticleSystem ripple;
    public Rigidbody rb;
    public GameObject rippleCamera;
    public float rippleInterval;

    private float lastRippleTimer;
    private float rippleIntervalTimer;

    // Update is called once per frame
    void Update()
    {
        rippleCamera.transform.position = transform.position + Vector3.up * 10;
        Shader.SetGlobalVector("_Player_Position", transform.position - 0.6f * transform.forward);
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

//    private void OnTriggerEnter(Collider other) 
//    {
//        if (other.gameObject.layer == 4 && Math.Abs(rb.velocity.y) > 0.4f)
//        {
//            createRipple(-180, 180, 3, 2, 2, 2);
//        }
//    }

    private void OnTriggerStay(Collider other)
    {
        rippleIntervalTimer = Time.time - lastRippleTimer;
        if (rippleIntervalTimer > rippleInterval)
        {
            if (other.gameObject.layer == 4 && Math.Sqrt(Math.Pow(rb.velocity.x, 2) + Math.Pow(rb.velocity.z, 2)) > 0.03f)
            {
                int y = (int) transform.eulerAngles.y;
                createRipple(y - 40, y + 40, 2, 5, 3, 3);
            }
            rippleIntervalTimer = 0;
            lastRippleTimer = Time.time;
        }
    }

//    private void OnTriggerExit(Collider other)
//    {
//        if (other.gameObject.layer == 4 && Math.Abs(rb.velocity.y) > 0.4f)
//        {
//            createRipple(-180, 180, 3, 2, 2, 2);
//        }
//    }
}
