using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [Space(5), Header("Prefab reference")]
    public Boid boidPrefab;
    List<Boid> boids = new List<Boid>();

    [Space(5), Header("Initialisation variables")]
    [Range(10, 500)] public int startingSize = 100;
    const float AgentDensity = 0.08f;

    [Space(5), Header("Movement variables")]
    [Range(1f, 100f)] public float maxSpeed = 10f;
    [Range(1f, 100f)] public float driveFactor = 10f;

    [Space(5), Header("Behaviour weight values")]
    [Range(0f, 25f)] public float alignmentWeight = 1f;
    [Range(0f, 25f)] public float cohesionWeight = 1f;
    [Range(0f, 25f)] public float separationWeight = 1f;
    [Range(0f, 25f)] public float stayInViewWeight = 1f;

    [Space(5), Header("Detection variables")]
    [Range(1f, 10f)] public float detectionRange = 3f;
    [Range(0f, 1f)] public float avoidanceRangeMult = 0.5f;

    // Private variabels
    private float _squareMaxSpeed;
    private float _squareNeighbourRange;
    private float _squareAvoidRange;
    public float squareAvoidanceRange { get { return _squareAvoidRange; } }
    public float squareMaxSpeed { get { return _squareMaxSpeed; } }


    // Start is called before the first frame update
    void Start() {
        _squareMaxSpeed = maxSpeed * maxSpeed;
        _squareNeighbourRange = detectionRange * detectionRange;
        _squareAvoidRange = _squareNeighbourRange * avoidanceRangeMult * avoidanceRangeMult;

        // Instantiate flock
        for (int i = 0; i < startingSize; i++) {
            Boid newBoid = Instantiate(
                boidPrefab,
                Random.insideUnitCircle * startingSize * AgentDensity,
                Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),
                transform
                );
            newBoid.name = "Agent " + i;
            boids.Add(newBoid);
        }
    }

    // Update is called once per frame
    void Update() {
        foreach (Boid boid in boids) {
            boid.Move();
        }
    }
}
