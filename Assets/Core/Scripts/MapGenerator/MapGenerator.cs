using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform mapFloor;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;
    public Vector2 maxMapSize;

    [Range(0,1)]
    public float outlinePercent;

    public float tileSize;

    List<Coord> allTileCoords;
    Queue<Coord> shuffleTilesCoords;
    Queue<Coord> shuffleOpenTilesCoords;
    Transform[,] tileMap;

    Map currentMap;   

    private void Start()
    {
        FindObjectOfType<Spawner>().OnNewWay += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);
        //GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.5f, currentMap.mapSize.y * tileSize);

        //Generating coords
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffleTilesCoords = new Queue<Coord>(Utility.ShuffleArray<Coord>(allTileCoords.ToArray(), currentMap.seed));
        //currentMap.mapCenter = new Coord((int)currentMap.mapSize.x / 2, (int)currentMap.mapSize.y / 2);


        //Create map holder
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;


        //Spawing tiles
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPostion(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90f)) as Transform;
                newTile.localScale = Vector3.one * (1-outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;
            }
        }

        
        //Spawing obstacles
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
        
        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != currentMap.mapCenter && MapIsFullAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleHieght = Mathf.Lerp(currentMap.minObstacleheight, currentMap.maxObstacleheight, (float) prng.NextDouble());
                Vector3 obstaclePosition = CoordToPostion(randomCoord.x, randomCoord.y);

                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHieght/2, Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3( (1 - outlinePercent) * tileSize, obstacleHieght, (1 - outlinePercent) * tileSize);

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);

                float colorPercent = randomCoord.y / (float) currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);

                obstacleRenderer.sharedMaterial = obstacleMaterial;
                allOpenCoords.Remove(randomCoord);
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }


        shuffleOpenTilesCoords = new Queue<Coord>(Utility.ShuffleArray<Coord>(allOpenCoords.ToArray(), currentMap.seed));

        //Creating the navmesh mask
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left *(currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform masRight= Instantiate(navmeshMaskPrefab, Vector3.right *(currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        masRight.parent = mapHolder;
        masRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform masTop= Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        masTop.parent = mapHolder;
        masTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y)/2f) * tileSize;

        Transform masBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        masBottom.parent = mapHolder;
        masBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;


        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);
    }

    bool MapIsFullAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {

        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1;

        while(queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if(x == 0 || y == 0)
                    {
                        if(neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if(!mapFlags[neighbourX, neighbourY] && !obstacleMap[ neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);

        return targetAccessibleTileCount == accessibleTileCount;
    }

    Vector3 CoordToPostion(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt( position.x / tileSize + (currentMap.mapSize.x - 1) / 2);
        int y = Mathf.RoundToInt( position.z / tileSize + (currentMap.mapSize.y - 1) / 2);

        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);

        return tileMap[x, y];
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffleTilesCoords.Dequeue();
        shuffleTilesCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffleOpenTilesCoords.Dequeue();
        shuffleOpenTilesCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }
        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    [System.Serializable]
    public class Map
    {

        public Coord mapSize;
        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleheight;
        public float maxObstacleheight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCenter
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}
