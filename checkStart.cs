using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkStart : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform player;
    private bool playerStart = false;
    EnemyAI[] enemyAIs;
    AIDestinationSetter[] aIDestinationSetters;
    void Start()
    {
        enemyAIs = FindObjectsOfType<EnemyAI>();
        aIDestinationSetters = FindObjectsOfType<AIDestinationSetter>();
    }

    void Update()
    {
        checkPoint();
        if (playerStart)
        {
            foreach (var item in enemyAIs)
            {
                item.start = true;
            }

            foreach (var item in aIDestinationSetters)
            {
                item.start = true;
            }
        }
        else
        {
            foreach (var item in enemyAIs)
            {
                item.start = false;
            }
            foreach (var item in aIDestinationSetters)
            {
                item.start = false;
            }
        }
    }

    private void checkPoint()
    {
        if(player.position.x > transform.position.x)
        {
            playerStart = true;
        }else playerStart = false;
    }
}
