using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class movement : NetworkBehaviour
{
    public float factor = .1F;
    void Start()
    {
        
    }

    void FixedUpdate() {
        if (IsServer) {
          float y = Convert.ToSingle(factor * Math.Sin(Time.frameCount / 10));
          Debug.Log("Updating pos");
          Vector3 offset = new Vector3(0, y, 0);
          transform.Translate(offset);
        }
    }
}
