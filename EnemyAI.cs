using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    public bool start = false;

    public bool roaming = true;
    private float moveSpeed = 6f;
    private float nextWPDistance = 0.6f;
        
    public Seeker seeker;
    public bool updateContinePath;
    public Transform player;

    //shoot
    public bool isShoot = false;
    public GameObject bullet;
    public float bulletSpeed;
    public float timeBtwFire;
    public bool isFlipped = false;
    private float fireCooldown;
    private float timeStart = 2f;
    private float timeStartTmp = 0f;

    bool reachDestination = false;
    Path path;
    Coroutine moveCoroutine;

    private void Start()
    {
        reachDestination = true;
    }

    private void Update()
    {
        Debug.Log(start);
        if (isShoot && start)
        {
            fireCooldown -= Time.deltaTime;

            if (fireCooldown < 0)
            {
                fireCooldown = timeBtwFire;
                CalculatePath();
                EnemyFireBullet();
            }
        }

        LookAtPlayer();
    }
    public void LookAtPlayer()
    {
        Vector3 flipped = transform.localScale;
        flipped.z *= -1f;

        if (transform.position.x > player.position.x && isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = false;

        }
        else if (transform.position.x < player.position.x && !isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = true;
        }
    }
    void EnemyFireBullet()
    {
        var bulletTmp = Instantiate(bullet, transform.position, Quaternion.identity);

        Rigidbody2D rb = bulletTmp.GetComponent<Rigidbody2D>();
        Vector3 playerPos = FindObjectOfType<MovingPlayer>().transform.position;
        Vector3 direction = playerPos - transform.position;
        rb.AddForce(direction.normalized * bulletSpeed, ForceMode2D.Impulse);
        Destroy(bulletTmp, 3.5f);
    }

    void CalculatePath()
    {
        Vector2 target = FindTarget();

        if (seeker.IsDone())
        {
            seeker.StartPath(transform.position, target, OnPathComplete);
        }
    }

     void OnPathComplete(Path p)
    {
        if (p.error) return;
        path = p;
        MoveToTarget();
    }

    void MoveToTarget()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveToTargetCoroutine());
    }

    IEnumerator MoveToTargetCoroutine()
    {
        int currentWP = 0;
        reachDestination = false;

        while (currentWP < path.vectorPath.Count)
        {
            Vector2 direction = ((Vector2)path.vectorPath[currentWP] - (Vector2)transform.position).normalized;
            Vector2 force = direction * moveSpeed * Time.deltaTime;
            transform.position += (Vector3)force;

            float distance = Vector2.Distance(transform.position, path.vectorPath[currentWP]);
            if(distance < nextWPDistance)
            {
                currentWP++;
            }

            yield return null;
        }
        reachDestination = false;
    }

    Vector2 FindTarget()
    {
        Vector3 playerPos = FindObjectOfType<MovingPlayer>().transform.position;
        if (roaming)
        {
            return (Vector2)playerPos + (Random.Range(10f, 25f) * new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)).normalized);
        }
        else
        {
            return playerPos;
        }
    }
}
