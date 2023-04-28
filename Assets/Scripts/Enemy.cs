using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public enum TypeEnemy
{
    Boss,
    Zombie,
}

public class Enemy : MonoBehaviour
{
    public TypeEnemy type;
    public int reward;
    [SerializeField]
    protected float maxSpeed;
    [SerializeField]
    protected float maxHP;

    protected float speed;
    protected float hp;
    private float slowedTimeRemain;

    [SerializeField]
    protected Animator animator;
    [SerializeField]
    private VisualEffect bloodEffect;

    private Vector3 target;
    private int wayPointIndex;

    private void Start()
    {
        Reset();
    }

    private void Update()
    {
        // Move to target point
        Vector3 dir = (target - transform.position).normalized;
        transform.Translate(Vector3.forward.normalized * speed * Time.deltaTime * GameController.instance.gameSpeed);
        Quaternion toQuaternion = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toQuaternion, 200 * speed * Time.deltaTime * GameController.instance.gameSpeed);

        // Get next wayPoint
        if (Vector3.Distance(transform.position, target) <= .4f)
        {
            GetNextWayPoint();
            return;
        }

        // Reset speed
        if (slowedTimeRemain <= 0 && !IsDied())
        {
            speed = maxSpeed;
            animator.speed = GameController.instance.gameSpeed;
        } else {
            slowedTimeRemain -= Time.deltaTime;
        }
    }

    private void GetNextWayPoint()
    {
        if (wayPointIndex >= GridSystem.points.Count - 1)
        {
            GameController.instance.RemoveEnemy(this);
            GameController.instance.AddReusedEnemy(this);
            gameObject.SetActive(false);
            return;
        }
        target = GridSystem.points[++wayPointIndex];
    }

    public void OnSlowed(float slowPercent, float slowTime)
    {
        speed = maxSpeed * (1 - slowPercent);
        animator.speed = GameController.instance.gameSpeed * (1 - slowPercent);
        slowedTimeRemain = slowTime;
    }

    public void OnDamaged(float damage)
    {
        hp -= damage;
        bloodEffect.Play();
        if (hp <= 0 && gameObject.activeSelf)
        {
            StartCoroutine(OnKilled());
        }
    } 

    // Coroutine on be killed
    private IEnumerator OnKilled()
    {
        speed = 0;
        animator.Play("Dying");
        RemoveTargetEffect();

        yield return new WaitForSeconds(4f);

        GameController.instance.AddReusedEnemy(this);
        GameController.instance.RemoveEnemy(this);
        gameObject.SetActive(false);
    }

    public void AttackKing()
    {
        hp = 0;
        RemoveTargetEffect();
        GameController.instance.AddReusedEnemy(this);
        GameController.instance.RemoveEnemy(this);
        gameObject.SetActive(false);
    }

    // Reset properties
    public void Reset()
    {
        hp = maxHP;
        speed = maxSpeed;
        wayPointIndex = 0;
        target = GridSystem.points[0];
        slowedTimeRemain = 0;
        gameObject.SetActive(true);
        animator.Play("Running");
    }

    // Return true is hp <= 0
    public bool IsDied()
    {
        return hp <= 0;
    }

    private void RemoveTargetEffect()
    {
        IEnumerable<VisualEffect> targetEffects = GetComponentsInChildren<VisualEffect>().Where(e => e.gameObject != gameObject);
        foreach (VisualEffect e in targetEffects)
        {
            e.SetBool("hasTarget", false);
        }
    }
}
