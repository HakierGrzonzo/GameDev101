using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // Start is called before the first frame update
    private int index;
    [SerializeField]
    private GameObject solider;
    private List<Solider> soldiers = new List<Solider>();
    [SerializeField]
    private float radius = 3F;
    private GameManager gm;
    private Vector3 intendedPosition;

    private float lastSpawnedAt = 0f;
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

    [ClientRpc]
    void UpdateSoldiersClientRpc() {
        this.soldiers = new List<Solider>(GetComponentsInChildren<Solider>());
    }

    void SpawnSolider() {
        float soliderPosFactor = 0.8F;
        Vector3 soliderPosition = this.transform.position * soliderPosFactor + Vector3.up;
        var soldierObj = Instantiate(this.solider, soliderPosition, this.transform.rotation);
        var soldier = soldierObj.GetComponent<Solider>();
        soldiers.Add(soldier);
        var networkObj = GetComponent<NetworkObject>();
        lastSpawnedAt = Time.timeSinceLevelLoad;
        var soldierNet = soldierObj.GetComponent<NetworkObject>();
        soldierNet.SpawnWithOwnership(networkObj.OwnerClientId);
        soldier.transform.parent = this.transform;
        UpdateSoldiersClientRpc();
    }

    [ServerRpc]
    void DoFirstTickInitServerRpc() {
        if (doneFirstTickInit) return;
        doneFirstTickInit = true;
        Debug.Log("DoingFirstTickInit");
        SpawnSolider();
    }
    // Update is called once per frame
    void Update()
    {
        if (!gm.isGameStarted) {
            return;
        }
        if (IsServer && Time.timeSinceLevelLoad - lastSpawnedAt > 10f) {
            SpawnSolider();
        }
        if (!IsOwner) {
            return;
        }
        DoFirstTickInitServerRpc();
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log(String.Format("Deactivating {0} soldiers", soldiers.Count));
            foreach (var s in soldiers)
            {
                s.isActive = false;
            }
        }
        if (Input.GetMouseButtonDown(0)) {
            Debug.Log(String.Format("Toggling {0} soldiers", soldiers.Count));
            foreach (var s in soldiers)
            {
                s.checkIfShouldActivate();
            }
        }
    }
}