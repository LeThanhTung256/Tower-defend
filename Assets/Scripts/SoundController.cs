using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController instance;

    [SerializeField]
    private AudioSource gameAudio;
    [SerializeField]
    private AudioClip gameWinSound;
    [SerializeField]
    private AudioClip gameBackgroundSound;

    // Enemies sound
    [SerializeField]
    private AudioSource enemiesWalkingSound;
    [SerializeField]
    private AudioSource enemiesGroupSound;

    private void Awake()
    {
        instance = this;
    }

    public void ControlEnemiesWalkingSound(bool isPlay)
    {
        if (isPlay && !enemiesWalkingSound.isPlaying)
            enemiesWalkingSound.Play();
        else if (!isPlay && enemiesWalkingSound.isPlaying)
            enemiesWalkingSound.Stop();
    }

    public void ControlEnemiesGroupSound(bool isPlay)
    {
        if (isPlay && Random.Range(0f, 1f) > 0.5 && !enemiesGroupSound.isPlaying)
        {
            enemiesGroupSound.Play();
            return;
        }

        if (!isPlay && enemiesGroupSound.isPlaying)
        {
            enemiesGroupSound.Stop();
        }
    }

    public void PlayBackgroundSound(bool isPlay)
    {
        if (isPlay)
            gameAudio.PlayOneShot(gameBackgroundSound);
        else
            gameAudio.Stop();    
    }

    public void PlayWinSound()
    {
        gameAudio.PlayOneShot(gameWinSound);
    }
}
