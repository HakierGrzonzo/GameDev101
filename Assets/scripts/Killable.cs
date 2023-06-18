using Unity.Netcode;
using UnityEngine;
public class Killable : NetworkBehaviour {
    [SerializeField]
    private int maxHp = 100;
    private int currentHp;

    private void Start() {
        currentHp = maxHp;
    }

    public void DoDamage(int amount) {
        if (amount > currentHp) {
            this.NetworkObject.Despawn();
        } else {
            currentHp -= amount;
        }
    }
}
