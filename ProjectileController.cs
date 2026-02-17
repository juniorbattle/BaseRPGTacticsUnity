
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Trajectory Parameters")]
    public float curveFactor = 0.5f; // Controls the curvature of the trajectory
    public float speed = 10f;        // Initial launch speed
    public float detectionRadius = 0.5f;  // Rayon de détection autour du projectile

    public bool isReached = false;
    public bool isMissed = false;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float journeyLength;
    private float startTime;

    public void LaunchProjectile(Vector3 target)
    {
        startPosition = this.transform.position;
        targetPosition = target;
        journeyLength = Vector3.Distance(startPosition, targetPosition);
        startTime = Time.time;
    }

    void Update()
    {
        // If no target set, do nothing
        if (startPosition == Vector3.zero || targetPosition == Vector3.zero) return;

        // Calculate normalized journey time
        float distanceCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distanceCovered / journeyLength;

        // Calculate base linear trajectory
        Vector3 linearTrajectory = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

        // Calculate vertical curve with custom curvature
        float height = CalculateVerticalCurve(fractionOfJourney);
        Vector3 curvedTrajectory = linearTrajectory + Vector3.up * height;

         // Détection des collisions avec un SphereCast
        Vector3 direction = curvedTrajectory - transform.position;
        float distanceToTravel = direction.magnitude;

        // Lancer un SphereCast pour détecter les obstacles (en filtrant sur le tag "Tile")
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, detectionRadius, direction.normalized, out hit, distanceToTravel))
        {
            // Vérifier si l'objet touché a le tag "Tile"
            if (hit.collider.CompareTag("Tile"))
            {
                HandleCollision(hit);
                return; // Si collision avec un obstacle "Tile", arrêter le projectile
            }
        }

        // Move projectile
        transform.position = curvedTrajectory;

        // Optional: Rotate projectile to face movement direction, considering curvature
        RotateProjectile(fractionOfJourney);

        // Check if projectile reached target
        if (fractionOfJourney >= 1f)
        {
            OnTargetReached();
        }
    }

    private float CalculateVerticalCurve(float t)
    {
        // Parabolic curve calculation with custom curvature
        // The curve peaks at the midpoint of the trajectory
        float peakHeight = curveFactor * journeyLength * 0.5f;
        return 4 * peakHeight * (t - t * t);
    }

    private void OnTargetReached()
    {
        // Implement your target hit logic here
        // For example: damage calculation, effect spawning, etc.
        Debug.Log("Projectile reached target!");

        isReached = true;

        // Optional: Destroy or deactivate projectile
        //Destroy(gameObject);
    }

    // Optional method to set trajectory parameters dynamically
    public void SetTrajectoryParameters(float angle, float curve, float projectileSpeed)
    {
        curveFactor = curve;
        speed = projectileSpeed;
    }

    private void RotateProjectile(float t)
    {
        // Calculate the tangent vector at the current point along the trajectory
        Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, t) + Vector3.up * CalculateVerticalCurve(t);

        // Small offset in time for tangent calculation
        float epsilon = 0.01f;
        Vector3 nextPosition = Vector3.Lerp(startPosition, targetPosition, t + epsilon) + Vector3.up * CalculateVerticalCurve(t + epsilon);

        // Calculate direction from current position to next position
        Vector3 tangent = (nextPosition - currentPosition).normalized;

        // Rotate the projectile to face the tangent direction (this is more suitable for an arrow-like projectile)
        if (tangent.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(tangent);
        }
    }

    private void HandleCollision(RaycastHit hit)
    {
        // Gérer la collision avec un obstacle ayant le tag "Tile"
        Debug.Log("Projectile a frappé un obstacle avec le tag 'Tile': " + hit.collider.gameObject.name);

        isMissed = true;

        // Détruire ou désactiver le projectile
        //Destroy(gameObject);
    }
}
