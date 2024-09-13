using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform; // Reference to the player's transform
    public Vector3 offset = new Vector3(0f, 10f, -10f); // Offset from the player

    void Update()
    {
        if (playerTransform != null)
        {
            // Set the camera's position to the player's position with the offset
            transform.position = new Vector3(playerTransform.position.x + offset.x, 
                                             transform.position.y, 
                                             playerTransform.position.z + offset.y);
        }
    }
}
