using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Match3GameManager : MonoBehaviour {

    Tile[,] grid;

    [SerializeField]
    int sizeX;
    
    public int sizeY;
    [SerializeField]
    Tile[] tilesPrefabs;

    bool CanMove = false;
    bool fast = true;

	void Start ()
    {
        grid = new Tile[sizeX, sizeY * 2];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                InstantiateTile(i,j);
            }
        }
        Check();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string s = "";
            for (int j = sizeY * 2 - 1; j >= 0; j--)
            {

                for (int i = sizeX -1; i >= 0; i--)
                {
                    if (grid[i, j] != null)
                        s += grid[i, j].name;
                    else
                        s += "NULL";
                }
                s += "\n";
            }
            print(s);
        }
    }

    int dragX = -1;
    int dragY = -1;
    public void Drag(Tile tile)
    {
        if (!CanMove)
            return;
        dragX = tile.x;
        dragY = tile.y;
    }
    public void Drop(Tile tile)
    {
        if (!CanMove)
            return;

        if (dragX == -1 || dragY == -1)
            return;

        SwapTiles(dragX, dragY, tile.x, tile.y);

        dragX = -1;
        dragY = -1;
    }
    void SwapTiles(int x1, int y1, int x2, int y2)
    {
        fast = false;
        if (x1 == x2 && y1 == y2)
            return;
        MoveTile(x1, y1, x2, y2);

        List<Tile> TilesToCheck = CheckHorizontalMatches();
        TilesToCheck.AddRange(CheckVerticalMatches());

        if (TilesToCheck.Count == 0)
        {
            MoveTile(x1, y1, x2, y2);
        }
        Check();
    }
    void Check()
    {
        List<Tile> TilesToDestroy = CheckHorizontalMatches();
        TilesToDestroy.AddRange(CheckVerticalMatches());

        TilesToDestroy = TilesToDestroy.Distinct().ToList();

        bool sw = TilesToDestroy.Count == 0;

        for (int i = 0; i < TilesToDestroy.Count; i++)
        {
            if (TilesToDestroy[i] != null)
            {
                Destroy(TilesToDestroy[i].gameObject);
                InstantiateTile(TilesToDestroy[i].x, TilesToDestroy[i].y + sizeY);
            }            
        }

        if(!sw)
            StartCoroutine(Gravity());
    }
    IEnumerator Gravity()
    {
        bool Sw = true;        
        while (Sw)
        {
            CanMove = false;
            Sw = false;
            for (int j = 0; j < sizeY * 2; j++)
            {
                for (int i = 0; i < sizeX; i++)
                {
                    //TODO CAER
                    if (Fall(i, j))
                    {
                        Sw = true;
                    }
                }
                
                if (j <= sizeY && !fast) //<-Wait
                    yield return null;
            }
        }
        yield return null;
        CanMove = true;
        Check();
        
    }
    bool Fall(int x, int y)
    {
        if (x < 0 || y <= 0 || x >= sizeX || y >= sizeY * 2) // <- SizeY * 2
            return false;
        if (grid[x, y] == null)
            return false;
        if (grid[x, y-1] != null)
            return false;
        
        MoveTile(x, y, x, y-1);
        return true;
    }
    List<Tile> CheckHorizontalMatches()
    {
        List<Tile> TilesToCheck = new List<Tile>();
        List<Tile> TilesToReturn = new List<Tile>();
        string Type = "";

        for (int j = 0; j < sizeY; j++)
        {
            for (int i = 0; i < sizeX; i++)
            {
                if (grid[i, j].type != Type)
                {
                    if (TilesToCheck.Count >= 3)
                    {
                        TilesToReturn.AddRange(TilesToCheck);
                    }
                    TilesToCheck.Clear();
                }
                Type = grid[i, j].type;
                TilesToCheck.Add(grid[i, j]);
            }

            if (TilesToCheck.Count >= 3)
            {
                TilesToReturn.AddRange(TilesToCheck);
            }
            TilesToCheck.Clear();
        }
        return TilesToReturn;
    }
    List<Tile> CheckVerticalMatches()
    {
        List<Tile> TilesToCheck = new List<Tile>();
        List<Tile> TilesToReturn = new List<Tile>();
        string Type = "";

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            { 
                if (grid[i, j].type != Type)
                {
                    if (TilesToCheck.Count >= 3)
                    {
                        TilesToReturn.AddRange(TilesToCheck);
                    }
                    TilesToCheck.Clear();
                }
                Type = grid[i, j].type;
                TilesToCheck.Add(grid[i,j]);
            }

            if (TilesToCheck.Count >= 3)
            {
                TilesToReturn.AddRange(TilesToCheck);
            }
            TilesToCheck.Clear();
        }
        return TilesToReturn;
    }
    void MoveTile(int x1, int y1, int x2, int y2)
    {
        if (grid[x1, y1] != null)
            grid[x1, y1].transform.position = new Vector3(x2, y2);

            
        if (grid[x2, y2] != null)        
            grid[x2, y2].transform.position = new Vector3(x1, y1);

            
        Tile temp = grid[x1, y1];
        grid[x1, y1] = grid[x2, y2];
        grid[x2, y2] = temp;

        if (grid[x1, y1] != null)
            grid[x1, y1].ChangePosition(x1, y1);
        if (grid[x2, y2] != null)
            grid[x2, y2].ChangePosition(x2, y2);

    }
    void InstantiateTile(int x, int y)
    {
        Tile go = Instantiate(
                    tilesPrefabs[Random.Range(0, tilesPrefabs.Length)],
                    new Vector3(x, y),
                    Quaternion.identity,
                    transform // <- go.transform.SetParent(transform) (Vieja version ); 
                    ) as Tile;

        go.Constructor(this, x, y);
        grid[x, y] = go;

    }    
}
