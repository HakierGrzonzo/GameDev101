using Unity.Netcode;
using UnityEngine;
public class Killable : NetworkBehaviour {
    [SerializeField]
    private int maxHp = 100;
    private int currentHp;

    [SerializeField]
    private GameObject onDeathSpawn;

    private void Start() {
        currentHp = maxHp;
    }

    [ClientRpc]
    public void PostDeathClientRpc() {
        Instantiate(onDeathSpawn, this.transform.position, this.transform.rotation);
    }

    public void DoDamage(int amount) {
        if (amount > currentHp) {
            PostDeathClientRpc();
            this.NetworkObject.Despawn();
        } else {
            currentHp -= amount;
        }
    }
}
