using UnityEngine;
using PathCreation;


public class SphereEnemy : MonoBehaviour
{
    [SerializeField] private int maxHP = 5; // Maximum hit points of the enemy
    private int currentHP; // Current hit points of the enemy

    [SerializeField] private Transform[] linearPoints; 
    [SerializeField] private Transform[] infinityPoints; 
    [SerializeField] private float moveSpeed = 5f; // Movement speed

    private Vector3 targetPosition; // The target position to move towards
    private int wayPointIndex=0;

    
    public float duration = 2f; // Duration of the movement

    private float t = 0f; // Parameter for interpolation

    public PathCreator pathCreator;
    public float speed = 5;
    private float distanceTraveled;


    private void Start()
    {
        currentHP = maxHP;
        
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log(currentHP);

        if (currentHP <= 0)
        {
            // Enemy defeated, perform necessary actions (e.g., play death animation, destroy object)
            Destroy(gameObject);
        }
    }
    
    private void Movement(Transform[] path)
    {
        if (wayPointIndex<= path.Length - 1)
        {
            //Debug.Log(wayPointIndex);
            targetPosition = path[wayPointIndex].position;
            // Calculate the direction to the target position
            Vector3 direction = targetPosition - transform.position;
            // Calculate the movement for the frame based on the direction and speed
            Vector3 movement = direction.normalized * (moveSpeed * Time.deltaTime);

            // Move the sphere towards the current waypoint
            transform.Translate(movement);

            // Check if the sphere has reached the current waypoint
            if (Vector3.Distance(transform.position, path[wayPointIndex].position) < 0.1f)
            {
                // Move to the next waypoint
                wayPointIndex++;
            }
        }
        else
        {
            wayPointIndex = 0;
        }
    }

    private void InfinityMovement(Transform[] path)
    {

        if (wayPointIndex< path.Length - 1)
        {
            // Increment the parameter based on time and duration
            t += Time.deltaTime / duration;
            t = Mathf.Clamp01(t); // Ensure t stays between 0 and 1

            // Calculate the position along the Bézier curve
            Vector3 position = CalculateBezierPoint(path[wayPointIndex].position, path[wayPointIndex+1].position, t);

            // Update the sphere's position
            transform.position = position;
            
            // Check if the sphere has reached the current waypoint
            if (t >= 1f)
            {
                // Move to the next waypoint
                wayPointIndex++;
                // Reset the parameter
                t = 0f;
            }
        }
        else
        {
            wayPointIndex = 0;
        }
    }
    
    private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, float t) //idk lol
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // (1-t)^3 * p0
        p += 3f * uu * t * p0; // 3(1-t)^2 * t * p0
        p += 3f * u * tt * p1; // 3(1-t) * t^2 * p1
        p += ttt * p1; // t^3 * p1

        return p;
    }

    private void Update()
    {
        distanceTraveled += speed * Time.deltaTime;
        transform.position = pathCreator.path.GetPointAtDistance(distanceTraveled);
    }
}
