using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    void Start()
    {
        // GameObject.Destroy(gameObject, .5f);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Character") || other.gameObject.CompareTag("Ground")) GameObject.Destroy(gameObject);
    }
}
