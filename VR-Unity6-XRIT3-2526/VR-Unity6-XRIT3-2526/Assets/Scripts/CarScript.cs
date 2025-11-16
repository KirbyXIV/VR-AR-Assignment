using System.Collections.Generic;
using UnityEngine;

public class CarScript : MonoBehaviour
{
    public List<Transform> cwps;
    public List<Transform> route;
    public int routeNumber = 0;
    public int targetWP;
    public bool go = false;
    public float initialDelay;
    public float maxSpeed = 7.5f;
    public float currentSpeed = 0f;
    public float acceleration = 4f;
    public float deceleration = 10f;
    public float obstacleSlowSpeed = 0f;
    private float targetSpeed;
    private bool obstacleDetected = false;

    // Traffic light variables
    private TrafficLightsController trafficLightController;
    private bool atRedLight = false;
    private int currentTrafficLight = -1;

    void Start()
    {
        cwps = new List<Transform>();
        for (int i = 1; i <= 12; i++)
            cwps.Add(GameObject.Find("CWP (" + i + ")").transform);

        // Find the traffic light controller in the scene
        trafficLightController = FindFirstObjectByType<TrafficLightsController>();

        SetRoute();
        initialDelay = Random.Range(2.0f, 15.0f);
        transform.position = new Vector3(0.0f, -5.0f, 0.0f);
        targetSpeed = maxSpeed;
    }

    void FixedUpdate()
    {
        if (!go)
        {
            initialDelay -= Time.deltaTime;
            if (initialDelay <= 0.0f) { go = true; SetRoute(); }
            else return;
        }
        Vector3 displacement = route[targetWP].position - transform.position;
        displacement.y = 0;
        float dist = displacement.magnitude;
        if (dist < 0.1f)
        {
            targetWP++;
            if (targetWP >= route.Count) { SetRoute(); return; }
        }

        // Determine target speed based on obstacles and traffic lights
        if (atRedLight) targetSpeed = 0f; // Stop at red light
        else if (obstacleDetected) targetSpeed = obstacleSlowSpeed;
        else targetSpeed = maxSpeed;

        if (currentSpeed < targetSpeed) currentSpeed += acceleration * Time.deltaTime;
        else if (currentSpeed > targetSpeed) currentSpeed -= deceleration * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);

        Vector3 velocity = displacement.normalized * currentSpeed;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.MovePosition(transform.position + velocity * Time.deltaTime);
        if (velocity != Vector3.zero)
        {
            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, velocity, 10.0f * Time.deltaTime, 0f);
            rb.MoveRotation(Quaternion.LookRotation(desiredForward));
        }
    }

    void SetRoute()
    {
        routeNumber = Random.Range(0, 4);
        switch (routeNumber)
        {
            case 0:
                route = new List<Transform> { cwps[0], cwps[1] };
                break;
            case 1:
                route = new List<Transform> { cwps[2], cwps[3] };
                break;
            case 2:
                route = new List<Transform> { cwps[7], cwps[4] };
                break;
            case 3:
                route = new List<Transform> { cwps[5], cwps[6] };
                break;
        }
        transform.position = new Vector3(route[0].position.x, 0.55f, route[0].position.z);
        targetWP = 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Pedestrian") || other.CompareTag("Car"))
        {
            obstacleDetected = true;
        }

        // Check for traffic light trigger and get which light it is
        if (other.CompareTag("TrafficLight"))
        {
            // Search up the hierarchy to find which TL this belongs to
            Transform current = other.transform;
            while (current != null)
            {
                if (current.name.Contains("TL (1)"))
                {
                    currentTrafficLight = 1;
                    break;
                }
                else if (current.name.Contains("TL (2)"))
                {
                    currentTrafficLight = 2;
                    break;
                }
                else if (current.name.Contains("TL (3)"))
                {
                    currentTrafficLight = 3;
                    break;
                }
                else if (current.name.Contains("TL (4)"))
                {
                    currentTrafficLight = 4;
                    break;
                }
                current = current.parent;
            }

            CheckTrafficLight();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Continuously check traffic light state while in trigger zone
        if (other.CompareTag("TrafficLight"))
        {
            CheckTrafficLight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Pedestrian") || other.CompareTag("Car"))
        {
            obstacleDetected = false;
        }

        // Clear red light flag when leaving traffic light zone
        if (other.CompareTag("TrafficLight"))
        {
            atRedLight = false;
            currentTrafficLight = -1;
        }
    }

    private void CheckTrafficLight()
    {
        if (trafficLightController != null && currentTrafficLight != -1)
        {
            int lightState = trafficLightController.GetLightState(currentTrafficLight);

            // If light is red (0), stop; if green (1), go
            atRedLight = (lightState == 0);
        }
    }
}