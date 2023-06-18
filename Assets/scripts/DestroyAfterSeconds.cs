using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    [SerializeField]
    private float timeToLive = 5;
    private float instancedAt = 0;
    // Start is called before the first frame update
    void Start()
    {
        instancedAt = Time.timeSinceLevelLoad;
    }

    // Update is called once per frame
    void Update()
    {
        var timeAlive = Time.timeSinceLevelLoad - instancedAt;
        if (timeAlive > timeToLive) {
            Destroy(gameObject);
        }
    }
}
