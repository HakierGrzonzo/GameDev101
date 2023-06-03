using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // Start is called before the first frame update
    private int index;
    [SerializeField]
    private GameObject solider;
    [SerializeField]
    private float radius = 3F;
    private GameManager gm;
    private Vector3 intendedPosition;
    private bool doneFirstTickInit = false;

    Vector3 GetPosition() {
        float playerAngle = (float) (2 * Math.PI) / (gm.NumberOfPlayers());
        float offset = playerAngle / 2;
        float angle = playerAngle * index + offset;
        return new Vector3((float) Math.Cos(angle) * radius, 0, (float) Math.Sin(angle) * radius);
    }

    public void UpdatePosition() {
        var newPosition = GetPosition();
        transform.position = newPosition;
        transform.LookAt(gm.gameObject.transform, Vector3.up);
    }

    void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
        if (IsServer) {
            index = gm.RegisterPlayer(this);
            if (index == -1) {
                throw new Exception("Failed to connect to already running game");
            }
            UpdatePosition();
        }
        if(IsOwner) {
            GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }

    [ServerRpc]
    void DoFirstTickInitServerRpc() {
        if (doneFirstTickInit) return;
        doneFirstTickInit = true;
        Debug.Log("DoingFirstTickInit");
        float soliderPosFactor = 0.8F;
        Vector3 soliderPosition = this.transform.position * soliderPosFactor + Vector3.up;
        var soliderObj = Instantiate(solider, soliderPosition, this.transform.rotation);
        var networkObj = GetComponent<NetworkObject>();
        soliderObj.GetComponent<NetworkObject>().SpawnWithOwnership(networkObj.OwnerClientId);
    }
    // Update is called once per frame
    void Update()
    {
        if (!gm.isGameStarted) {
            return;
        }
        if (!IsOwner) {
            return;
        }
        DoFirstTickInitServerRpc();

    }
}