using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    public GameObject SawPrefab;
    public GameObject StompBlockPrefab;
    public GameObject MovingBlockPrefab;
    
    public Texture2D[] LevelPatterns;
    public Tilemap LevelTilemap;
    public TileBase WallTile;
    
    private Vector3Int _lastPatternPoint = new Vector3Int(4, 7, 0);

    private Color normalYellow = new(1, 1, 0);
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateRandomPattern();
        GenerateRandomPattern();
        GenerateRandomPattern();
    }

    public void GenerateRandomPattern()
    {
        var patternID = Random.Range(0, LevelPatterns.Length);
        GeneratePattern(patternID);
    }

    void GeneratePattern(int patternID)
    {
        var startPoint = _lastPatternPoint;
        startPoint.x = -5;
        
        // Стены для отрезка
        LevelTilemap.SetTile(new Vector3Int(-5, startPoint.y, 0), WallTile);
        LevelTilemap.SetTile(new Vector3Int(4, startPoint.y, 0), WallTile);
        startPoint.y -= 1;
        
        LevelTilemap.SetTile(new Vector3Int(-5, startPoint.y, 0), WallTile);
        LevelTilemap.SetTile(new Vector3Int(4, startPoint.y, 0), WallTile);
        startPoint.y -= 1;
        
        var pattern = LevelPatterns[patternID];
        // Debug.Log("Паттерн: " + patternID);
        
        // Потому что отсчет текстуры идет как в шейдерах от левого нижнего угла
        for (int y = pattern.height-1; y >= 0; y--)
        {
            // Левая стена
            LevelTilemap.SetTile(startPoint, WallTile);
            startPoint.x++;
            
            // Шаблон
            for (int x = 0; x < pattern.width; x++)
            {
                GenerateTile(pattern, new Vector2Int(x, y), startPoint);
                startPoint.x++;
            }
            
            // Правая стена
            LevelTilemap.SetTile(startPoint, WallTile);
            startPoint.x = -5;
            startPoint.y--;
        }
        // Debug.Log("======");

        _lastPatternPoint = startPoint;
    }

    void GenerateTile(Texture2D pattern, Vector2Int patternPosition, Vector3Int worldPosition)
    {
        Color pixelColor = pattern.GetPixel(patternPosition.x, patternPosition.y);
        
        // Debug.Log("x: " + patternPosition.x + ", y: " + patternPosition.y + " color: " + pixelColor);

        // Если пиксель черный (альфа-канал игнорируем или проверяем)
        // Используем Color.black или проверяем яркость
        if (pixelColor == Color.black) LevelTilemap.SetTile(worldPosition, WallTile);
        else if (pixelColor == Color.red) SpawnPrefab(SawPrefab, worldPosition);
        else if (pixelColor == normalYellow) SpawnPrefab(StompBlockPrefab, worldPosition);
        else if (pixelColor == Color.green) SpawnMovingBlock(MovingBlock.MovementDirection.Horizontal, worldPosition);
        else if (pixelColor == Color.cyan) SpawnMovingBlock(MovingBlock.MovementDirection.Vertical, worldPosition);
        
        // Если пиксель прозрачный или белый — ничего не делаем (остается пустота)
    }

    void SpawnMovingBlock(MovingBlock.MovementDirection direction, Vector3Int cell)
    {
        var mb = SpawnPrefab(MovingBlockPrefab, cell).GetComponent<MovingBlock>();
        mb.ChangeDirection(direction);
    }

    GameObject SpawnPrefab(GameObject prefabToSpawn, Vector3Int cell)
    {
        // 1. Получаем центр тайла в мировых координатах
        var worldPosition = LevelTilemap.GetCellCenterWorld(cell);
        
        /*
        if (prefabToSpawn == SawPrefab)
            worldPosition.y -= 0.16f;
            */

        // 2. Спавним GameObject в этой точке
        var spawnedObj = Instantiate(prefabToSpawn, worldPosition, Quaternion.identity);
        
        // 3. (Опционально) Делаем объект дочерним для Tilemap, чтобы не захламлять иерархию
        spawnedObj.transform.parent = LevelTilemap.transform;
        
        return spawnedObj;
    }
}
