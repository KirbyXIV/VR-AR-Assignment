using UnityEngine;

public class CarNoiseArea : MonoBehaviour
{
    public CarScript carScript;

    void Start()
    {

    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (carScript != null)
            {
                carScript.SetPlayerInNoiseArea(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (carScript != null)
            {
                carScript.SetPlayerInNoiseArea(false);
            }
        }
    }
}