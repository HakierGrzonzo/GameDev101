using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private List<Player> players = new List<Player>();
    public bool isGameStarted = false;

    private int playersThatDied = 0;

    [SerializeField]
    private GameObject StartGamePrefab;
    [SerializeField]
    private GameObject EndGamePrefab;

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

    [ClientRpc]
    void GameOverClientRpc() {
        Instantiate(EndGamePrefab, this.transform.position + Vector3.up, Quaternion.LookRotation(Vector3.down, Vector3.forward));
    }

    public void KillPlayer(ulong playerId) {
        if (this.NetworkManager.ConnectedClients.TryGetValue(playerId, out var client)) {
            foreach (var soldierNet in client.PlayerObject.GetComponentsInChildren<NetworkObject>()) {
                try {
                    soldierNet.Despawn();
                } catch {}
            }
            try {
                client.PlayerObject.Despawn();
            } catch {}
            playersThatDied++;
        }
        Debug.Log(playersThatDied);
        if (playersThatDied == NumberOfPlayers() -1) {
            GameOverClientRpc();
        }
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
