using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class itemCollector : MonoBehaviour
{
    private int gems = 0;
    [SerializeField] private Text scoreText;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("gem"))
        {
            Destroy(collision.gameObject);
            gems++;
            scoreText.text = ("Score: " + gems.ToString());
        }
    }

    public void resetScore()
    {
        scoreText.text = "Score: 0";
    }
}
