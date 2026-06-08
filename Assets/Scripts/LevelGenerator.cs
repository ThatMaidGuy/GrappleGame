using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    public GameObject SawPrefab;
    public GameObject StompBlockPrefab;
    public GameObject MovingBlockPrefab;
    public GameObject DronePrefab;
    
    public Texture2D[] LevelPatterns;
    public Tilemap LevelTilemap;
    public TileBase WallTile;
    
    private Vector3Int _lastPatternPoint = new Vector3Int(4, 1, 0);
    // Храним ID последнего сгенерированного паттерна
    private int _lastPatternID = -1; 

    private Color normalYellow = new(1, 1, 0);
    
    void Start()
    {
        // Первый паттерн жестко задан как 0
        _lastPatternID = 0;
        GeneratePattern(_lastPatternID);
        
        // Генерируем следующий случайный
        GenerateRandomPattern();
    }

    public void GenerateRandomPattern()
    {
        // Если паттерн всего один, защищаем код от бесконечного цикла
        if (LevelPatterns.Length <= 1)
        {
            GeneratePattern(0);
            return;
        }

        int patternID;
        
        // Выбираем новый ID до тех пор, пока он совпадает с предыдущим
        do
        {
            patternID = Random.Range(0, LevelPatterns.Length);
        } 
        while (patternID == _lastPatternID);

        // Запоминаем новый паттерн как последний использованный
        _lastPatternID = patternID;
        
        GeneratePattern(patternID);
    }

    // Остальная часть вашего кода (GeneratePattern, GenerateTile и т.д.) остается без изменений
    void GeneratePattern(int patternID)
    {
        var startPoint = _lastPatternPoint;
        startPoint.x = -5;
        
        LevelTilemap.SetTile(new Vector3Int(-5, startPoint.y, 0), WallTile);
        LevelTilemap.SetTile(new Vector3Int(4, startPoint.y, 0), WallTile);
        startPoint.y -= 1;
        
        LevelTilemap.SetTile(new Vector3Int(-5, startPoint.y, 0), WallTile);
        LevelTilemap.SetTile(new Vector3Int(4, startPoint.y, 0), WallTile);
        startPoint.y -= 1;
        
        var pattern = LevelPatterns[patternID];
        
        for (int y = pattern.height-1; y >= 0; y--)
        {
            LevelTilemap.SetTile(startPoint, WallTile);
            startPoint.x++;
            
            for (int x = 0; x < pattern.width; x++)
            {
                GenerateTile(pattern, new Vector2Int(x, y), startPoint);
                startPoint.x++;
            }
            
            LevelTilemap.SetTile(startPoint, WallTile);
            startPoint.x = -5;
            startPoint.y--;
        }

        _lastPatternPoint = startPoint;
    }

    void GenerateTile(Texture2D pattern, Vector2Int patternPosition, Vector3Int worldPosition)
    {
        Color pixelColor = pattern.GetPixel(patternPosition.x, patternPosition.y);
        
        if (pixelColor == Color.black) LevelTilemap.SetTile(worldPosition, WallTile);
        else if (pixelColor == Color.red) SpawnPrefab(SawPrefab, worldPosition);
        else if (pixelColor == normalYellow) SpawnPrefab(StompBlockPrefab, worldPosition);
        else if (pixelColor == Color.green) SpawnMovingBlock(MovingBlock.MovementDirection.Horizontal, worldPosition);
        else if (pixelColor == Color.cyan) SpawnMovingBlock(MovingBlock.MovementDirection.Vertical, worldPosition);
        else if (pixelColor == Color.magenta) SpawnPrefab(DronePrefab, worldPosition);
    }

    void SpawnMovingBlock(MovingBlock.MovementDirection direction, Vector3Int cell)
    {
        var mb = SpawnPrefab(MovingBlockPrefab, cell).GetComponent<MovingBlock>();
        mb.ChangeDirection(direction);
    }

    GameObject SpawnPrefab(GameObject prefabToSpawn, Vector3Int cell)
    {
        var worldPosition = LevelTilemap.GetCellCenterWorld(cell);
        var spawnedObj = Instantiate(prefabToSpawn, worldPosition, Quaternion.identity);
        spawnedObj.transform.parent = LevelTilemap.transform;
        return spawnedObj;
    }
}