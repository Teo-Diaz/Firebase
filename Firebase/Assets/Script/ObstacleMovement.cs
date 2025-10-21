using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private bool hasScored = false;

    private Transform playerTransform; 
    private float scoreDistance = 0.5f; 

    void Start()
    {
        // Find the player's transform
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

        float distanceToPlayer = playerTransform.position.x - transform.position.x;
        if (!hasScored && distanceToPlayer > scoreDistance)
        {
            hasScored = true;
            FindObjectOfType<ScoreManager>().IncreaseScore(1);
        }
        if (transform.position.x < -10f)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        hasScored = false;
    }
}
