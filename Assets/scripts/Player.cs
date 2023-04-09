using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // Start is called before the first frame update
    private int index;
    [SerializeField]
    private float radius = 3F;
    private GameManager gm;
    private Vector3 intendedPosition;

    Vector3 GetPosition() {
        float playerAngle = (float) (2 * Math.PI) / (gm.NumberOfPlayers());
        float offset = playerAngle / 2;
        float angle = playerAngle * index + offset;
        return new Vector3((float) Math.Cos(angle) * radius, 0, (float) Math.Sin(angle) * radius);
    }

    public void UpdatePosition() {
        var newPosition = GetPosition();
        transform.position = newPosition;
    }
    void Start()
    {
        if (IsServer) {
            gm = FindFirstObjectByType<GameManager>();
            index = gm.RegisterPlayer(this);
            UpdatePosition();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
