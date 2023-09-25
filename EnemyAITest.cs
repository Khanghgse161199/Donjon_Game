using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.U2D;
using static UnityEngine.GraphicsBuffer;

public class EnemyBat_AI : MonoBehaviour
{
    public SpriteRenderer sprite;
    public Seeker seeker;
    public Transform target;
    public float moveSpeed = 2f;
    public float nextWayPointDistance = 2f;
    public float repeatTimeUpdatePath = 0.5f;

    Path path;
    Coroutine moveCoroutine;

    // Part 10
    //public float freezeDurationTime;
    //float freezeDuration;
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        CaculatePath();
    }

    void CaculatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(transform.position, target.position, OnPathCallBack);
        }
    }

    void OnPathCallBack(Path p)
    {
        if (!path.error)
        {
            path = p;
            MoveToTarget();
        };
    }

    void MoveToTarget()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveToTargetCoroutine());
    }

    IEnumerator MoveToTargetCoroutine()
    {
        int currentWP = 0;
        while (currentWP < path.vectorPath.Count)
        {
            Vector2 direction = ((Vector2)path.vectorPath[currentWP] - (Vector2)transform.position).normalized;
            Vector3 force = direction * moveSpeed * Time.deltaTime;
            transform.position += force;

            float distance = Vector2.Distance(transform.position, path.vectorPath[currentWP]);
            Debug.Log(distance + " test");
            if (distance < nextWayPointDistance)
                currentWP++;

            if (force.x != 0)
                if (force.x < 0)
                    sprite.transform.localScale = new Vector3(-1, 1, 0);
                else
                    sprite.transform.localScale = new Vector3(1, 1, 0);

            yield return null;
        }
    }
}
