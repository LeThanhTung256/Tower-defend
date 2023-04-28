using System.Collections;
using UnityEngine;
using TMPro;

public class SpawnEnemies : MonoBehaviour
{
    public static SpawnEnemies instance;
    [SerializeField]
    private Enemy bossPrefab;
    [SerializeField]
    private Enemy zombiePrefab;
    private Vector3 spawnPosition;
    [SerializeField]
    private TextMeshProUGUI time;
    [SerializeField]
    private TextMeshProUGUI wave;

    // Số lượng enemy trong một lượt
    private int numOfEnemy;

    // Số lượng wave, và khoảng cách thời gian giữa các wave
    private int numOfWave;
    private int waveIndex;
    private float timeWave;

    private float countdown;
    private bool isFinishSpawn;

    private void Awake()
    {
        instance = this;

        spawnPosition = transform.position + new Vector3(-.25f, 0, -.25f);
        timeWave = 9.9f;
        numOfWave = 7;
    }

    private void Start()
    {
        Restart();
    }

    private void Update()
    {
        if (waveIndex <= numOfWave)
        {
            if (countdown <= 0f)
            {
                StartCoroutine(SpawnWave());
                countdown = timeWave;
                waveIndex++;

                wave.text = (waveIndex < numOfWave) ? $"{waveIndex}/{numOfWave}" : "Last wave";
            }

            time.text = (waveIndex < numOfWave) ? $"{Mathf.Floor(countdown)}" : "";
            countdown -= Time.deltaTime * GameController.instance.gameSpeed;
        }
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < numOfEnemy - 1; i++)
        {
            SpawnEnemy(TypeEnemy.Zombie);
            yield return new WaitForSeconds(1f);
        }

        SpawnEnemy(TypeEnemy.Boss);

        if (waveIndex == numOfWave)
            isFinishSpawn = true;

        numOfEnemy++;
    }

    private void SpawnEnemy(TypeEnemy type)
    {
        Enemy enemy = GameController.instance.GetReusedEnemy(type);
        if (enemy == null)
        {
            if (type == TypeEnemy.Zombie)
            {
                enemy = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                enemy = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
            }
        }
        else
        {
            enemy.transform.position = spawnPosition;
            enemy.Reset();
        }

        GameController.instance.AddEnemy(enemy);
    }

    public void Restart()
    {
        waveIndex = 0;
        numOfEnemy = 3;
        countdown = 5f;
        wave.text = $"{waveIndex}/{numOfWave}";
        isFinishSpawn = false;
    }


    public bool IsFinishSpawn()
    {
        return isFinishSpawn;
    }
}
