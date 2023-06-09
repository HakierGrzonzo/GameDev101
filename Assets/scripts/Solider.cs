using Unity.Netcode;
using UnityEngine;

public class Solider : NetworkBehaviour
{
    [SerializeField]
    private float speed = .1F;
    public bool isActive = true;
    private Rigidbody rb;
    [SerializeField]
    private float clickSensitivity;

    private Vector3? targetPos = null;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
    }
}
