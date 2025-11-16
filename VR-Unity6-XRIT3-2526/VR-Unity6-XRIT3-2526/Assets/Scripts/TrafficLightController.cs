using UnityEngine;

public class TrafficLightsController : MonoBehaviour
{
    // Declare public variables for the traffic light objects and their child lights
    public Transform t1;
    public Transform t2;
    public Transform t3;
    public Transform t4;
    public GameObject t1green;
    public GameObject t1red;
    public GameObject t2green;
    public GameObject t2red;
    public GameObject t3green;
    public GameObject t3red;
    public GameObject t4green;
    public GameObject t4red;

    // Declare variables for managing the state and the timer
    public float stateTimer;
    public int state;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the traffic light objects and their child lights
        t1 = transform.Find("TL (1)");
        t2 = transform.Find("TL (2)");
        t3 = transform.Find("TL (3)");
        t4 = transform.Find("TL (4)");

        t1green = t1.Find("Green light").gameObject;
        t1red = t1.Find("Red light").gameObject;

        t2green = t2.Find("Green light").gameObject;
        t2red = t2.Find("Red light").gameObject;

        t3green = t3.Find("Green light").gameObject;
        t3red = t3.Find("Red light").gameObject;
        
        t4green = t4.Find("Green light").gameObject;
        t4red = t4.Find("Red light").gameObject;

        // Initialize state and timer
        stateTimer = 10.0f;  // Timer starts at 10 seconds
        SetState(1);  // Initial state
    }

    // Update is called once per frame
    void Update()
    {
        // Decrease the timer by time.deltaTime
        stateTimer -= Time.deltaTime;

        // If the timer reaches zero, change the light state
        if (stateTimer <= 0)
        {
            // Toggle between state 1 and state 2
            if (state == 1)
            {
                SetState(2);
            }
            else
            {
                SetState(1);
            }

            // Reset the timer to 10 seconds
            stateTimer = 10.0f;
        }
    }

    // Function to set the state of the traffic lights
    void SetState(int c)
    {
        state = c;

        if (c == 1)
        {
            // State 1: TL1 green, TL2 and TL3 red
            t1green.SetActive(true);
            t1red.SetActive(false);
            t2green.SetActive(false);
            t2red.SetActive(true);
            t3green.SetActive(false);
            t3red.SetActive(true);
            t4green.SetActive(true);
            t4red.SetActive(false);
        }
        else
        {
            // State 2: TL1 red, TL2 and TL3 green
            t1green.SetActive(false);
            t1red.SetActive(true);
            t2green.SetActive(true);
            t2red.SetActive(false);
            t3green.SetActive(true);
            t3red.SetActive(false);
            t4green.SetActive(false);
            t4red.SetActive(true);
        }
    }
    public int GetLightState(int lightIndex)
    {
        switch (lightIndex)
        {
            case 1: return t1green.activeSelf ? 1 : 0;
            case 2: return t2green.activeSelf ? 1 : 0;
            case 3: return t3green.activeSelf ? 1 : 0;
            case 4: return t4green.activeSelf ? 1 : 0;
            default: return -1; // Invalid light index
        }
    }
}
