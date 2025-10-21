using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    private bool scored = false;

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !scored)
        {
            scored = true;
            FindObjectOfType<ScoreManager>().IncreaseScore(1);
        }
    }

    private void OnEnable()
    {
        scored = false; // Reset the scored flag when the obstacle is reused
    }
}
