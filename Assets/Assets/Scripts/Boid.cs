using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Boid : MonoBehaviour {
    // Variables //
    [Header("Movement variables")]
    public float speed;
    public Vector2 movementDir;
    private Vector2 _normalizedDir;

    public float smoothTime = 0.85f;
    Vector2 curVelocity;

    // Detection variables
    public List<Transform> neighbours = new List<Transform>();
    private Collider2D _collider;

    // Reference to manager
    private FlockManager _fM;

    // Variables to keep boids in the area
    private Vector2 _center;
    private float _radius;

    // Initialize some things
    void Start() {
        // Get collider
        _collider = GetComponent<BoxCollider2D>();

        // Get the flock object for setting private variables
        _fM = transform.parent.GetComponent<FlockManager>();

        // Set values
        _center = Vector2.zero;
        _radius = 35f;
    }

    // Update //
    public void Move() {
        // Get all the neighbouring boids 
        neighbours = GetNeighbours();
        this.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.green, Color.red, neighbours.Count / 16f);

        // Calculate all the modifiers
        var alignment = ComputeAlignment();
        var cohesion = ComputeCohesion();
        var separation = ComputeSeparation();
        var sIV = StayInView();

        // Calculate the movement vector with the modifier values
        movementDir.x += (alignment.x * _fM.alignmentWeight) + (cohesion.x * _fM.cohesionWeight) + 
            (separation.x * _fM.separationWeight) + (sIV.x * _fM.stayInViewWeight);
        movementDir.y += (alignment.y * _fM.alignmentWeight) + (cohesion.y * _fM.cohesionWeight) + 
            (separation.y * _fM.separationWeight) + (sIV.y * _fM.stayInViewWeight);

        // Create holder and check if we are going too fast
        movementDir = movementDir * _fM.driveFactor;
        if (movementDir.sqrMagnitude > _fM.squareMaxSpeed)
            movementDir = movementDir.normalized * _fM.maxSpeed;

        transform.up = movementDir.normalized;
        transform.position += (Vector3)movementDir * Time.deltaTime;
    }

    // Flocking functions //
    List<Transform> GetNeighbours() {
        // Create a returnable list and overlap circle
        List<Transform> context = new List<Transform>();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _fM.detectionRange);

        // Go through each object found with the overlap circle
        foreach(Collider2D c in colliders) {
            if (c != _collider) { // If a collider there isn't our collider add it to the list
                context.Add(c.transform);
            }
        }

        // Return list
        return context;
    }
    // Function to make boid stay in a certain radius
    Vector2 StayInView() {
        Vector2 centerOffset = _center - (Vector2)transform.position;
        float t = centerOffset.magnitude / _radius;

        if (t < 0.9f)
            return Vector2.zero;

        return centerOffset * t * t;
    }

    #region Behaviour Functions
    Vector2 ComputeAlignment() {
        // If no neighbours, return current movement direction
        if (neighbours.Count == 0)
            return transform.up;

        // Add all points together and average
        Vector2 aMove = Vector2.zero;
        foreach (Transform boid in neighbours) {
            aMove += (Vector2)boid.transform.up;
        }
        aMove /= neighbours.Count;

        // Return alignment vector
        return aMove;
    }

    // Calculate the direction to middle point of all neighbours
    Vector2 ComputeCohesion() {
        // If no neighbours, return (0, 0)
        if (neighbours.Count == 0)
            return Vector2.zero;

        // Add all points together and average
        Vector2 cMove = Vector2.zero;
        foreach (Transform boid in neighbours) {
            cMove += (Vector2)boid.position;
        }
        cMove /= neighbours.Count;

        // Create offset from agent position
        cMove -= (Vector2)transform.position;
        cMove = Vector2.SmoothDamp(transform.up, cMove, ref curVelocity, smoothTime);
        return cMove;
    }

    Vector2 ComputeSeparation() {
        // If no neighbours, return no adjustment
        if (neighbours.Count == 0)
            return Vector2.zero;

        // Add all points together
        Vector2 sMove = Vector2.zero;
        int nAvoid = 0;
        foreach (Transform boid in neighbours) {
            if (Vector2.SqrMagnitude(boid.position - transform.position) < _fM.squareAvoidanceRange) {
                nAvoid++;
                sMove += (Vector2)(transform.position - boid.position);
            }
        }

        // Average the vector
        if (nAvoid > 0)
            sMove /= nAvoid;

        return sMove;
    }
    #endregion

    // Gizmo function
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Ray ray = new Ray(transform.position, (_normalizedDir * _fM.detectionRange));
        Gizmos.DrawRay(ray);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _fM.detectionRange);
    }
}
