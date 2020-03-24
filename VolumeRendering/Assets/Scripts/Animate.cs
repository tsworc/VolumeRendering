using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animate : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        DrawFaces df = GetComponent<DrawFaces>();

        //slice the volume based on total time
        df.boundsMax.x = Mathf.Sin(Time.fixedTime)/2+.5f;

        //spin the volume
        transform.Rotate(Vector3.forward, 0.8f);
    }
}
