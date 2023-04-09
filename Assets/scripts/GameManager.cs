using System.Collections.Generic;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    private List<Player> players = new List<Player>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public int RegisterPlayer(Player player) {
        players.Add(player);
        for (int i = 0; i < players.Count - 1; i++) {
            var p = players[i];
            p.UpdatePosition();
        }
        PostNewPlayerAddClientRpc(NumberOfPlayers());
        return players.Count - 1;
    }

    [ClientRpc]
    void PostNewPlayerAddClientRpc(int newNumberOfPlayers) {
        GetComponent<ProceduralPolyMesh>().GenerateMesh(newNumberOfPlayers * 3);
    }

    public int NumberOfPlayers() {
        return players.Count;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
