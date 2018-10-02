using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public bool devMode;

    public Wave[] waves;
    public Enemy enemy;

    LeavingEntity playerEntity;
    Transform playerT;

    Wave currentWave;
    int currentWaveNumber;

    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    MapGenerator map;

    float timeBetweencampingChecks = 2;
    float campThersholdDistance = 1.5f;
    float nextCampingCheckTime;
    Vector3 campPositionOld;
    bool isCamping;

    bool isDisable;

    public event System.Action<int> OnNewWay;

    private void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;
        playerEntity.OnDeath += OnPlayerDied;

        nextCampingCheckTime = timeBetweencampingChecks + Time.time;
        campPositionOld = playerT.position;

        map = GameObject.FindObjectOfType<MapGenerator>();
        NextWave();
    }

    private void Update()
    {
        if (isDisable)
            return;

        if(Time.time > nextCampingCheckTime)
        {
            nextCampingCheckTime = timeBetweencampingChecks + Time.time;

            isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThersholdDistance);
            campPositionOld = playerT.position;
        }

        if((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
        {
            enemiesRemainingToSpawn--;
            nextSpawnTime = Time.time + currentWave.timeBetweenmSpawns;

            StartCoroutine("SpawnEnemy");
            
        }

        if (devMode)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine("SpawnEnemy");
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                    GameObject.Destroy(enemy.gameObject);
                NextWave();
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1f;
        float tileFlashSpeed = 4;

        Transform randomTile = map.GetRandomOpenTile();
        if (isCamping)
            randomTile = map.GetTileFromPosition(playerT.position);

        Material tileMat = randomTile.GetComponent<Renderer>().material;
        Color initialColor = Color.white;// tileMat.color;
        Color flashColor = Color.red;
        float spawnTimer = 0f;


        while(spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            
            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, randomTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);
    }

    void OnPlayerDied()
    {
        isDisable = true;
    }

    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;
        if (enemiesRemainingAlive == 0)
            NextWave();
    }

    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up *3;
    }

    void NextWave()
    {
        currentWaveNumber++;
        
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if (OnNewWay != null)
                OnNewWay(currentWaveNumber);

            ResetPlayerPosition();
        }
    }

    [System.Serializable]
    public class Wave
    {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenmSpawns;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color skinColor;

    }

}
