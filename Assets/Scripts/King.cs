using UnityEngine;
using UnityEngine.UI;

public class King : MonoBehaviour
{
    private int maxHearts;
    private int remainLife;

    private Image[] hearts;
    [SerializeField]
    private GameObject heartsScreen;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip injuring;
    [SerializeField]
    private AudioClip dying;

    private void Awake()
    {
        maxHearts = 3;
        hearts = new Image[heartsScreen.transform.childCount];
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i] = heartsScreen.transform.GetChild(i).gameObject.GetComponent<Image>();
        }
    }

    private void Start()
    {
        NewGame();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            remainLife -= 1;
            hearts[remainLife].color = Color.black;
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            enemy.AttackKing();

            if (remainLife <= 0)
            {
                animator.Play("KingDying");
                audioSource.PlayOneShot(dying);
                GameController.instance.GameOver();
            } else
            {
                audioSource.PlayOneShot(injuring);
                animator.Play("KingReact");
            }
        }
    }

    public void NewGame()
    {
        remainLife = maxHearts;
        animator.Play("KingIdle");

        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].color = Color.red;
        }
    }
}
