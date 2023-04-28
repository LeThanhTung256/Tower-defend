using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.EventSystems;
using TMPro;

public enum TowerPrice
{
    Wizard = 200,
    Archer = 100,
} 

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public float gameSpeed;
    private int coin;

    // Screen
    [SerializeField]
    private GameObject gameOverScreen;
    [SerializeField]
    private GameObject gameWinScreen;
    [SerializeField]
    private GameObject confirmBuyTowerScreen;
    [SerializeField]
    private Animator notificationAnimator;
    [SerializeField]
    private TextMeshProUGUI notificationText;

    // Game object
    [SerializeField]
    private King king;
    [SerializeField]
    private SpawnEnemies gate;
    [SerializeField]

    // UI for buying tower
    private TextMeshProUGUI coinText;

    // Tower prefabs
    [SerializeField]
    private Tower wizardPrefab;
    [SerializeField]
    private Tower archerPrefab;

    // Manager enemies and towers
    private List<Enemy> enemies;
    private List<Enemy> reusedEnemies;
    private List<Tower> towers;

    // Buy tower
    private TypeTower selectTowerToBuy;
    private Position selectedPosition;
    private bool isBuyingTower;
    private bool justSelectTowerType;

    // Sound effect
    private bool isPlayingEnemyWalkingSound;

    // VFX
    [SerializeField]
    private VisualEffect attackRangeEffect;

    // Text
    private readonly string notEnoughCoinStr = "You don't have enough coin to buy this tower";
    private readonly string positionIsUsed = "Position is used";

    private void Awake()
    {
        instance = this;

        // fps
        Application.targetFrameRate = 60;

        enemies = new List<Enemy>();
        reusedEnemies = new List<Enemy>();
        towers = new List<Tower>();
        attackRangeEffect = Instantiate(attackRangeEffect);
    }

    private void Start()
    {
        NewGame();
    }

    private void Update()
    {
        coinText.text = $"Coin: ${coin}";

        // Check if win game
        if (SpawnEnemies.instance.IsFinishSpawn() && enemies.Count == 0 && !gameWinScreen.activeSelf)
        {
            WinGame();
        }

        // Selecting position
        if (isBuyingTower && Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000) && hit.transform.CompareTag("Position"))
            {
                Position position = hit.transform.GetComponent<Position>();

                // Check if position is used
                if (position.IsUsed())
                {
                    Debug.Log(positionIsUsed);
                    notificationText.text = positionIsUsed;
                    notificationAnimator.Play("ShowNotification");
                }  
                // Confirm buying tower 
                else if (selectedPosition != position)
                {
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(position.transform.position);
                    confirmBuyTowerScreen.transform.position = screenPos;
                    confirmBuyTowerScreen.SetActive(true);

                    selectedPosition?.UnSelect();
                    selectedPosition = position;
                    selectedPosition.Selecting();
                    attackRangeEffect.transform.position = selectedPosition.transform.position;
                    attackRangeEffect.Play();
                } 
            } else if (!justSelectTowerType && !EventSystem.current.alreadySelecting)
            {
                CancelBuying();
            }
        }

        justSelectTowerType = false;

        // Play Sound Effect
        if (enemies.FindIndex(e => !e.IsDied()) >= 0 && !isPlayingEnemyWalkingSound && gameSpeed != 0)
        {
            SoundController.instance.ControlEnemiesWalkingSound(true);
            SoundController.instance.ControlEnemiesGroupSound(true);
            isPlayingEnemyWalkingSound = true;
        }

        if (enemies.FindIndex(e => !e.IsDied()) < 0)
        {
            SoundController.instance.ControlEnemiesWalkingSound(false);
            SoundController.instance.ControlEnemiesGroupSound(false);
            isPlayingEnemyWalkingSound = false;
        }
    }

    // Select type tower to new tower
    public void SelectTypeTower(int type)
    {
        TypeTower selectedType = (TypeTower)type;

        // Check if not enough coin to buy
        if (!IsEnoughCoin(selectedType))
        {
            Debug.LogWarning(notEnoughCoinStr);
            notificationText.text = notEnoughCoinStr;
            notificationAnimator.Play("ShowNotification");
            CancelBuying();
            return;
        }

        // If click button select type again
        if (selectTowerToBuy == (TypeTower)type)
        {
            CancelBuying();
            return;
        }

        StartCoroutine(SelectTowerType((TypeTower)type));
    }

    private IEnumerator SelectTowerType(TypeTower type)
    {
        float attackRange = (type == TypeTower.Wizard) ? wizardPrefab.GetAttackRange() : archerPrefab.GetAttackRange();
        attackRangeEffect.SetFloat("AttackRange", attackRange);
        justSelectTowerType = true;
        GridSystem.instance.SelectingPosition();
        selectedPosition?.Selecting();
        yield return new WaitForSeconds(0.1f);
        selectTowerToBuy = type;
        isBuyingTower = true;
    }

    // Confirm buy tower
    public void BuyTower()
    {
        if (selectTowerToBuy == TypeTower.Null || selectedPosition == null)
        {
            CancelBuying();
            return;
        }

        Debug.Log("Complete buy tower ");
        isBuyingTower = false;
        confirmBuyTowerScreen.SetActive(false);

        Tower tower = (selectTowerToBuy == TypeTower.Wizard) ? wizardPrefab : archerPrefab;
        coin -= tower.Price;
        Vector3 newPosition = selectedPosition.transform.position + new Vector3(0, .2f, 0);
        Tower newTower = Instantiate(tower, newPosition, Quaternion.identity, selectedPosition.transform);
        towers.Add(newTower);

        GridSystem.instance.CompeleteSelect();
        selectedPosition.Use();
        selectedPosition = null;
        selectTowerToBuy = TypeTower.Null;
        isBuyingTower = false;
        attackRangeEffect.Stop();
    }

    public void CancelBuying()
    {
        Debug.Log("Cancel buy tower");
        isBuyingTower = false;
        selectTowerToBuy = TypeTower.Null;
        selectedPosition = null;

        EventSystem.current.SetSelectedGameObject(null);
        GridSystem.instance.CompeleteSelect();
        confirmBuyTowerScreen.SetActive(false);
        attackRangeEffect.Stop();
    }

    // Check if can buy tower
    private bool IsEnoughCoin(TypeTower selectedType)
    {
        return (selectedType == TypeTower.Wizard && coin >= wizardPrefab.Price) ||
               (selectedType == TypeTower.Archer && coin >= archerPrefab.Price);
    }

    public void GameOver()
    {
        gameSpeed = 0;
        gameOverScreen.SetActive(true);
        confirmBuyTowerScreen.SetActive(false);

        SoundController.instance.ControlEnemiesWalkingSound(false);
        SoundController.instance.ControlEnemiesGroupSound(false);
        SoundController.instance.PlayBackgroundSound(false);
        isPlayingEnemyWalkingSound = false;
    }

    private void WinGame()
    {
        gameSpeed = 0;
        gameWinScreen.SetActive(true);
        confirmBuyTowerScreen.SetActive(false);

        SoundController.instance.ControlEnemiesWalkingSound(false);
        SoundController.instance.PlayBackgroundSound(false);
        SoundController.instance.PlayWinSound();
    }

    public void AddEnemy(Enemy enemy)
    {
        enemies.Add(enemy);
    }

    public void AddReusedEnemy(Enemy enemy)
    {
        reusedEnemies.Add(enemy);
    }

    public void RemoveEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
    }

    public Enemy GetReusedEnemy(TypeEnemy type)
    {
        if (reusedEnemies.Count == 0)
        {
            return null;
        }

        Enemy enemy = reusedEnemies.Find(e => e.type == type);
        reusedEnemies.Remove(enemy);
        return enemy;
    }

    public List<Enemy> GetEnemies(Vector3 position, float range)
    {
        List<Enemy> enemiesInRange = new List<Enemy>();

        foreach(Enemy enemy in enemies)
        {
            if (Vector3.Distance(enemy.transform.position, position) <= range && !enemy.IsDied())
            {
                enemiesInRange.Add(enemy);
            }
        }

        return enemiesInRange;
    }

    public void ReceiveCoin(int receivedCoin)
    {
        coin += receivedCoin;
    }

    public void NewGame()
    {
        coin = 200;
        isBuyingTower = false;
        selectTowerToBuy = TypeTower.Null;

        gameOverScreen.SetActive(false);
        gameWinScreen.SetActive(false);
        confirmBuyTowerScreen.SetActive(false);

        // Reuse enemy
        while (enemies.Count > 0)
        {
            AddReusedEnemy(enemies[0]);
            enemies[0].gameObject.SetActive(false);
            RemoveEnemy(enemies[0]);
        }

        // Clear all tower
        while (towers.Count > 0)
        {
            Tower tower = towers[0];
            towers.RemoveAt(0);
            Destroy(tower.gameObject);
        }

        CancelBuying();

        // Play sound
        SoundController.instance.PlayBackgroundSound(true);

        // Create evironment
        GridSystem.instance.Reset();

        // Restart king and gate
        king.NewGame();
        gate.Restart();

        gameSpeed = 1;
        isPlayingEnemyWalkingSound = false;
    }
}
