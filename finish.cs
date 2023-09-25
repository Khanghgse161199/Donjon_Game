using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class finish : MonoBehaviour
{
    EnemyAI enemyAIs;
    DogEnemy dogEnemy;
    private bool levelComplete = false; 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        enemyAIs = FindAnyObjectByType<EnemyAI>();
        dogEnemy = FindAnyObjectByType<DogEnemy>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player" && !levelComplete)
        {
            Invoke("CompleteLevel", 1f);
            levelComplete = true;
        }
    }
    private void CompleteLevel()
    {
        if (enemyAIs == null && dogEnemy == null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
