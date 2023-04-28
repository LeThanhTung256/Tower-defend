using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public enum TypeTower
{
    Wizard = 0,
    Archer = 1,
    Null = -1,
}

public class Tower : MonoBehaviour
{
    public TypeTower type;
    public int Price
    {
        get { return price; }
    }

    private int price;

    [SerializeField]
    protected float attackRange;
    [SerializeField]
    protected float attackSpeed;
    [SerializeField]
    protected float damage;
    [SerializeField]
    protected float slowTime;
    [SerializeField]
    protected float slowPercent;

    [SerializeField]
    protected int maxTargets;
    [SerializeField]
    public List<Enemy> targets;
    protected float attackCountDown;
    [SerializeField]
    protected Animator animator;

    [SerializeField]
    protected Transform spawnPosVFX;
    [SerializeField]
    protected VisualEffect attackEfPrefab;
    [SerializeField]
    public List<VisualEffect> attackEffects;
    [SerializeField]
    protected VisualEffect targetEfPrefab;
    public List<VisualEffect> targetEffects;

    // Sound
    public AudioSource audioSource;
    public AudioClip attackSound;

    private void Awake()
    {
        targets = new List<Enemy>(maxTargets);
        attackEffects = new List<VisualEffect>(maxTargets);
        targetEffects = new List<VisualEffect>(maxTargets);

        for (int i = 0; i < maxTargets; i++)
        {
            VisualEffect attackEffect = Instantiate(attackEfPrefab, spawnPosVFX);
            VisualEffect targetEffect = Instantiate(targetEfPrefab, spawnPosVFX);
            targetEffect.SetBool("hasTarget", false);
            targetEffect.Play();
            attackEffects.Add(attackEffect);
            targetEffects.Add(targetEffect);
        }
    }

    private void Start()
    {

        attackCountDown = 0;
        // Rotate to gate spawn enemy pos
        Vector3 dir = (SpawnEnemies.instance.transform.position - transform.position).normalized;
        transform.forward = dir;
    }

    private void Update()
    {
        // Update enemy base on attackRange
        for (int i = 0; i < targets.Count;)
        {
            if (
                (Vector3.Distance(targets[i].transform.position, transform.position) > attackRange) ||
                !targets[i].gameObject.activeSelf ||
                targets[i].IsDied()
                )
            {
                // Remove target effect
                VisualEffect[] visualEffects = targets[i].GetComponentsInChildren<VisualEffect>();
                foreach(VisualEffect v in visualEffects)
                {
                    int indexItem = targetEffects.FindIndex(e => e == v);
                    if (indexItem >= 0)
                    {
                        v.SetBool("hasTarget", false);
                        break;
                    }    
                }

                targets.Remove(targets[i]);
                continue;
            }

            i++;
        }

        if (targets.Count < maxTargets)
        {
            List<Enemy> enemiesInRange = GameController.instance.GetEnemies(transform.position, attackRange);
            for (int i = 0; i < enemiesInRange.Count; i++)
            {
                if (targets.Count >= maxTargets)
                    break;

                if (targets.FindIndex(e => e == enemiesInRange[i]) >= 0)
                    continue;

                Enemy enemy = enemiesInRange[i];
                targets.Add(enemy);

                SetTargetEffect(enemy);
            }
        }

        // Rotate to enemy
        if (targets.Count > 0)
        {
            Vector3 dir = targets[0].transform.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = lookRotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }

        // Attack enemy
        int index = targets.FindIndex(e => !e.IsDied());
        if (targets.Count > 0 && attackCountDown <= 0 && index >= 0)
        {
            Attack();
        }

        // Update attackCountDown
        attackCountDown -= Time.deltaTime * GameController.instance.gameSpeed;
    }

    // Run animation, animation will call impact function in class effect
    private void Attack()
    {
        animator.Play("Attack");
        attackCountDown = attackSpeed;
    }

    // Impact on enemy
    public void Impact()
    {
        for (int i = 0; i < targets.Count;)
        {
            Enemy enemy = targets[i];
            enemy.OnSlowed(slowPercent, slowTime);
            enemy.OnDamaged(damage);

            if (enemy.IsDied())
            {
                OnKillEnemy(enemy);
                continue;
            }

            i++;
        }
    }

    // When enemy be killed, update controller enemies, reused enemies, coin and targets   
    private void OnKillEnemy(Enemy enemy)
    {
        int coin = enemy.reward;
        targets.Remove(enemy);
        GameController.instance.ReceiveCoin(coin);
    }

    private void SetTargetEffect(Enemy enemy)
    {
        VisualEffect targetEffect = targetEffects.Find(e => !e.GetBool("hasTarget"));

        if (enemy.IsDied() && !enemy.transform && !targetEffect?.transform)
            return;

        targetEffect.transform.SetParent(enemy.transform);
        targetEffect.transform.localPosition = Vector3.zero;
        targetEffect.transform.rotation = Quaternion.identity;
        targetEffect.SetBool("hasTarget", true);
        targetEffect.Play();
    }

    public float GetAttackRange()
    { return attackRange; }
}
