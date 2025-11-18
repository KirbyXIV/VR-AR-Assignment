using System.Collections;
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

    private TrafficLightsController trafficLightController;
    private bool atRedLight = false;
    private int currentTrafficLight = -1;

    private AudioSource audioSource;
    public AudioClip engineSound;
    public AudioClip honkSound;
    public float honkCooldown = 3f;
    public float honkRange = 10f;
    private float lastHonkTime = -999f;
    private Transform playerTransform;

    void Start()
    {
        cwps = new List<Transform>();
        for (int i = 1; i <= 13; i++)
            cwps.Add(GameObject.Find("CWP (" + i + ")").transform);

        trafficLightController = FindFirstObjectByType<TrafficLightsController>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = engineSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 5f;
        audioSource.maxDistance = 50f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        SetRoute();
        initialDelay = Random.Range(1.0f, 5f);
        transform.position = new Vector3(0.0f, -5.0f, 0.0f);
        targetSpeed = maxSpeed;
    }

    void FixedUpdate()
    {
        if (!go)
        {
            initialDelay -= Time.deltaTime;
            if (initialDelay <= 0.0f)
            {
                go = true;
                SetRoute();

                if (engineSound != null && !audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
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

        if (atRedLight)
        {
            targetSpeed = 0f;
        }
        else if (obstacleDetected)
        {
            targetSpeed = obstacleSlowSpeed;
        }
        else
        {
            targetSpeed = maxSpeed;
        }

        if (currentSpeed < targetSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
        else if (currentSpeed > targetSpeed)
        {
            currentSpeed -= deceleration * Time.deltaTime;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);

        Vector3 velocity = displacement.normalized * currentSpeed;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.MovePosition(transform.position + velocity * Time.deltaTime);
        if (velocity != Vector3.zero)
        {
            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, velocity, 10.0f * Time.deltaTime, 0f);
            rb.MoveRotation(Quaternion.LookRotation(desiredForward));
        }

        if (audioSource.isPlaying && engineSound != null)
        {
            audioSource.pitch = Mathf.Lerp(0.8f, 1.5f, currentSpeed / maxSpeed);
        }
    }

    void SetRoute()
    {
        switch (routeNumber)
        {
            case 0:
                route = new List<Transform> { cwps[12], cwps[9], cwps[4] };
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

        if (routeNumber >= 4)
        {
            routeNumber = 0;
        }
        else
        {
            routeNumber++;
        }
    }

    private bool IsPlayerNearby()
    {
        if (playerTransform == null)
            return false;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= honkRange;
    }

    private void TryHonk()
    {
        if (Time.time - lastHonkTime >= honkCooldown && honkSound != null && IsPlayerNearby())
        {
            audioSource.PlayOneShot(honkSound);
            lastHonkTime = Time.time;
        }
    }

    private bool IsInFront(Vector3 objectPosition)
    {
        Vector3 toObject = objectPosition - transform.position;
        float dot = Vector3.Dot(transform.forward, toObject.normalized);
        return dot > 0.5f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.IsChildOf(transform))
        {
            return;
        }

        if (other.CompareTag("Player") || other.CompareTag("Pedestrian") || other.CompareTag("Car"))
        {
            if (IsInFront(other.transform.position))
            {
                obstacleDetected = true;

                if (other.CompareTag("Player") || other.CompareTag("Pedestrian"))
                {
                    TryHonk();
                }
            }
        }

        if (other.CompareTag("TrafficLight"))
        {
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
        if (other.CompareTag("TrafficLight"))
        {
            CheckTrafficLight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.IsChildOf(transform))
        {
            return;
        }

        if (other.CompareTag("Player") || other.CompareTag("Pedestrian") || other.CompareTag("Car"))
        {
            obstacleDetected = false;
        }

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

            atRedLight = (lightState == 0);
        }
    }
}