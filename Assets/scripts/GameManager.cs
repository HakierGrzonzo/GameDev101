using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private List<Player> players = new List<Player>();
    protected bool isGameStarted = false;

    [SerializeField]
    private GameObject StartGamePrefab;
    // Start is called before the first frame update
    void Start()
    {

    }

    public int RegisterPlayer(Player player)
    {
        if (isGameStarted)
        {
            return -1;
        }
        players.Add(player);
        for (int i = 0; i < players.Count - 1; i++)
        {
            var p = players[i];
            p.UpdatePosition();
        }
        var newNumberOfPlayers = NumberOfPlayers();
        PostNewPlayerAddClientRpc(newNumberOfPlayers);
        if (newNumberOfPlayers == 2 && IsServer)
        {
            //Show player
            Instantiate(StartGamePrefab, this.transform.position + Vector3.up, Quaternion.LookRotation(Vector3.down, Vector3.forward));
        }
        return players.Count - 1;
    }

    [ClientRpc]
    void PostNewPlayerAddClientRpc(int newNumberOfPlayers)
    {
        GetComponent<ProceduralPolyMesh>().GenerateMesh(newNumberOfPlayers * 3);
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        isGameStarted = true;
        Debug.Log("Game started");
    }

    public int NumberOfPlayers()
    {
        return players.Count;
    }

    public bool isGameReadyToStart()
    {
        return players.Count >= 2;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
