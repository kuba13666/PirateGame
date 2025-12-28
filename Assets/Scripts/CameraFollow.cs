using UnityEngine;

/// <summary>
/// Makes the camera smoothly follow the player ship
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("The target to follow (player ship)")]
    public Transform target;

    [Tooltip("How smoothly the camera follows (lower = smoother)")]
    public float smoothSpeed = GameConstants.CAMERA_SMOOTH_SPEED;

    [Tooltip("Offset from the target position")]
    public Vector3 offset = new Vector3(0, 0, GameConstants.CAMERA_OFFSET_Z);

    void LateUpdate()
    {
        // Make sure we have a target
        if (target == null) return;

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;

        // Smoothly interpolate between current and desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Update camera position
        transform.position = smoothedPosition;
    }
}
