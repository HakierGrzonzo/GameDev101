using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Solider : NetworkBehaviour
{
    [SerializeField]
    private float speed = .1F;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (IsOwner)
        {
            GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }

    Vector3 GetDesiredPos()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = Camera.main.transform.position.y;
        var target = Camera.main.ScreenToWorldPoint(mousePos);
        Debug.Log(String.Format("{0} pos -> {1}", mousePos, target));
        target.y = 0;
        return target;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(GetDesiredPos(), 0.1f);
        Gizmos.DrawLine(GetDesiredPos(), this.transform.position);
    }

    [ServerRpc]
    void SetForceServerRpc(Vector3 difference)
    {
        difference.Normalize();
        rb.AddForce(difference * speed, ForceMode.VelocityChange);
    }

    void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        if (!Application.isFocused)
        {
            return;
        }
        var mouseWorldCoord = GetDesiredPos();
        var difference = mouseWorldCoord - this.transform.position;
        Debug.Log(difference);
        SetForceServerRpc(difference);
    }
}
