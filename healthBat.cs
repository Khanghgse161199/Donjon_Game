using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthBat : MonoBehaviour, IDamge
{
    public bool isShooting;

    private float health;
    private void Start()
    {
        if (isShooting)
        {
            health = 4f;
        }
        else health = 6f;
    }
    void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }

        //Debug.Log(health);
    }

    public void ApplyDamage(float amount, bool isDamage)
    {
        health -= amount;
    }
}
