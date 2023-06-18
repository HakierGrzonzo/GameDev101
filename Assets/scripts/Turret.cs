using System.Linq;
using Unity.Netcode;
using UnityEngine;
public class Turret : NetworkBehaviour {
    private float lastShotAt = 0;

    [SerializeField]
    private float shotInterval = 3;

    [SerializeField]
    private float range = 0.5f;

    [SerializeField]
    private float maxSpeedToShoot = 0.2f;

    [SerializeField]
    private int damage = 20;

    private Rigidbody rb;

    [SerializeField]
    private GameObject shotPrefab;

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }

    private bool CanShoot() {
        return rb.velocity.magnitude < maxSpeedToShoot;
    }

#nullable enable
    private Killable? GetTarget () {
        var candidates = Physics.OverlapSphere(this.transform.position, range);
        var soldiers = candidates.Where(candidate => candidate.CompareTag("soldier"));
        var ourNetworkObject = GetComponent<NetworkObject>();
        var enemies = soldiers.Where(soldier => 
            soldier.GetComponent<NetworkObject>().OwnerClientId != ourNetworkObject.OwnerClientId
        );

        if (enemies.Count() == 0) {
            return null;
        }

        var closestEnemy = enemies.OrderBy(enemy => (
            enemy.transform.position - transform.position).magnitude
        ).ElementAt(0);

        return closestEnemy.GetComponent<Killable>();
    }

    private void TryDoShot() {
        if (!CanShoot()) {
            return;
        }
        var target = GetTarget();

        if (target is Killable enemy) {
            DeliverShotServerRpc(enemy.NetworkObject);
            lastShotAt = Time.timeSinceLevelLoad;
        }
    }

    [ServerRpc]
    private void DeliverShotServerRpc(NetworkObjectReference target) {
        if(target.TryGet(out var netObject)) {
            netObject.GetComponent<Killable>().DoDamage(damage);
            PostShotClientRpc(netObject.transform.position);
        } else {
            Debug.LogError("Failed to resolve Killable");
        }
    }

    [ClientRpc]
    private void PostShotClientRpc(Vector3 targetPos) {
        GetComponent<AudioSource>().Play();
        var shot = Instantiate(shotPrefab, Vector3.zero, this.transform.rotation);
        var shotLine = shot.GetComponent<LineRenderer>();
        var coords = new Vector3[] {
            this.transform.position,
            targetPos,
        };
        shotLine.SetPositions(coords);
        var lineColor = IsOwner ? Color.blue : Color.red;
        shotLine.startColor = lineColor;
        shotLine.endColor = lineColor;

    }

    private void OnDrawGizmos() {
        if (CanShoot()) {
            Gizmos.DrawWireSphere(transform.position, range);
            var target = GetTarget();
            if (target is Killable enemy) {
                Gizmos.DrawLine(this.transform.position, enemy.transform.position);
            }
        }
    }

    private void FixedUpdate() {
        if (!IsOwner) {
            return;
        }
        var timeSinceLastShot = Time.timeSinceLevelLoad - lastShotAt;
        if (timeSinceLastShot > shotInterval) {
            TryDoShot();
        }
    }
}
