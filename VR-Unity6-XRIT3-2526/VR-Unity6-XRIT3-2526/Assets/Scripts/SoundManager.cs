using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource audioSource;

    public string triggerTag = "Player";
    public bool loopWhileInside = true;
    public bool stopOnExit = true;
    public AudioClip[] audioClips;
    public float timeBetweenClips = 2f;

    private bool isPlaying = false;
    private float timer = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.Stop();
    }

    void Update()
    {
        if (isPlaying)
        {
            timer += Time.deltaTime;

            if (timer >= timeBetweenClips)
            {
                PlayRandomClip();
                timer = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            if (!isPlaying)
            {
                PlayRandomClip();
                isPlaying = true;
                timer = 0f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            if (stopOnExit && isPlaying)
            {
                audioSource.Stop();
                isPlaying = false;
                timer = 0f;
            }
        }
    }

    private void PlayRandomClip()
    {
        if (audioClips.Length > 0)
        {
            int randomIndex = Random.Range(0, audioClips.Length);
            audioSource.clip = audioClips[randomIndex];
            audioSource.Play();
        }
    }
}