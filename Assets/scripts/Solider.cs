using System;
using Unity.Netcode;
using UnityEngine;

public class Solider : NetworkBehaviour
{
    [SerializeField]
    private float speed = .1F;
    public bool isActive = true;
    private Rigidbody rb;
    private ProceduralPolyMesh playboard;
    [SerializeField]
    private float clickSensitivity;

    private Vector3? targetPos = null;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playboard = FindFirstObjectByType<ProceduralPolyMesh>();
    }

    Vector3 GetDesiredPos()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = Camera.main.transform.position.y - this.transform.position.y;
        var target = Camera.main.ScreenToWorldPoint(mousePos);
        target.y = 0;
        return target;
    }

    void OnDrawGizmos()
    {
        if (!isActive || !IsOwner) {
            return;
        }
        var desiredPos = GetDesiredPos();
        desiredPos.y = this.transform.position.y;
        Gizmos.DrawSphere(desiredPos, 0.1f);
        Gizmos.DrawLine(desiredPos, this.transform.position);
    }

    [ServerRpc]
    void SetForceServerRpc(Vector3 difference)
    {
        difference.Normalize();
        rb.AddForce(difference * speed, ForceMode.VelocityChange);
    }

    [ServerRpc]
    void DieServerRpc() {
        this.NetworkObject.Despawn();
    }

    void Update() {
        if (IsOwner)
        {
            GetComponent<MeshRenderer>().material.color = isActive ? Color.green : Color.red;
        }
    }

    public void checkIfShouldActivate() {
        var mousePos = GetDesiredPos();
        mousePos.y = this.transform.position.y;
        var difference = mousePos - this.transform.position;
        if (difference.magnitude < clickSensitivity) {
            isActive = !isActive;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!IsServer) {
            return;
        }

        var gm = other.gameObject;
        Debug.Log(String.Format("Collision {0}", gm.tag));
        if (!gm.CompareTag("Player")) {
            return;
        }

        var ourPlayerId = this.NetworkObject.OwnerClientId;
        var collisionPlayerId = gm.GetComponent<NetworkObject>().OwnerClientId;
        if (collisionPlayerId == ourPlayerId) {
            return;
        }
        FindFirstObjectByType<GameManager>().KillPlayer(collisionPlayerId);
    }

    void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0) && isActive) {
            targetPos = GetDesiredPos();
        }
        if (targetPos is Vector3 target) {
            var difference = target - this.transform.position;
            SetForceServerRpc(difference);
            var lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.enabled = difference.magnitude > 0.1;
            var normalizedTarget = new Vector3(target.x, this.transform.position.y, target.z);
            // this array declaration is stupid
            var positions = new Vector3[] {this.transform.position, normalizedTarget};
            lineRenderer.SetPositions(positions);
        }
        if (playboard.IsOutOfBounds(this.transform.position)) {
            DieServerRpc();
        }
    }
}
