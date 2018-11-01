﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Tile
{
    public enum tileType
    {
        Grassland,
        Marsh,
        Mountain,
        Water
    }
    public enum tileResource
    {
        Wood,
        Stone,
        Food,
        Nothing
    }

    public tileType type;
    public tileResource resource;
    public int amountOfResource;
    public bool isWalkable;

}

public class TileMap : MonoBehaviour {

    //public GameObject selectedUnit;
    public TileType[] tileTypes;
    public TileResource[] tileResource;


    public Tile[,] map;


    int[,] tiles;    //tile types
    Node[,] graph;  //who every tile is touchiung


    MouseManagerS mouseManagerS;
    public GameObject selectedUnit;


    public int MapSizeX = 20;
    public int MapSizeY = 20;




    void Start()
    {
        mouseManagerS = GameObject.Find("MouseManager").GetComponent<MouseManagerS>();
        generateMap();
        generateGraphHelp();
        generateMapVisuals();
    }




    public string GetTileResName(int x, int y)
    {
        string ResorceNam = null;
        Tile.tileResource ans = map[x, y].resource;

        if(ans == Tile.tileResource.Wood)
        {
            ResorceNam = "Wood";
        }
        if (ans == Tile.tileResource.Stone)
        {
            ResorceNam = "Stone";
        }
        if (ans == Tile.tileResource.Food)
        {
            ResorceNam = "Food";
        }

        if (ans == Tile.tileResource.Nothing)
        {
            ResorceNam = "Nothing";
        }

        return ResorceNam;
    }



    public int GetTileResAmt(int x, int y)
    {
        
        int ans = map[x, y].amountOfResource;

        return ans;
    }

    public void gatherResource(int x, int y, int amount)
    {
        map[x, y].amountOfResource -= amount;
    }

    public void removeResource(int x, int y)
    {
        map[x, y].resource = Tile.tileResource.Nothing;
        string name = "Resource" + x.ToString() + "," + y.ToString();
        Destroy(GameObject.Find(name));
    }



    void generateMap()
    {
        //allocation of map tiles
        tiles = new int[MapSizeX, MapSizeY];



        map = new Tile[MapSizeX, MapSizeY];

        for (int x = 0; x < MapSizeX; x++)
        {
            for (int y = 0; y < MapSizeY; y++)
            {
                float height = GetHeight(x, y);
                if (height < .35)
                {
                    map[x, y] = new Tile();
                    map[x, y].type = Tile.tileType.Water;
                    map[x, y].isWalkable = false;
                }
                else if (height < .4)
                {
                    map[x, y] = new Tile();
                    map[x, y].type = Tile.tileType.Marsh;
                    map[x, y].isWalkable = true;
                }
                else if (height < .7)
                {
                    map[x, y] = new Tile();
                    map[x, y].type = Tile.tileType.Grassland;
                    map[x, y].isWalkable = true;
                }
                else
                {
                    map[x, y] = new Tile();
                    map[x, y].type = Tile.tileType.Mountain;
                    map[x, y].isWalkable = false;
                }
            }
        }

        GenerateResources();
    }

    void GenerateResources()
    {

        for (int x = 0; x < MapSizeX; x++)
        {
            for (int y = 0; y < MapSizeY; y++)
            {
                int rand = Random.Range((int)0, (int)15);
                if (rand == 0)
                {
                    map[x, y].resource = Tile.tileResource.Food;
                    if (CheckIfNextToMountain(x, y))
                    {
                        map[x, y].resource = Tile.tileResource.Stone;
                    }
                    map[x, y].amountOfResource = GenerateAmountOfResource();
                }
                else if (rand == 1)
                {
                    map[x, y].resource = Tile.tileResource.Stone;
                    map[x, y].amountOfResource = GenerateAmountOfResource();
                }
                else if (rand == 2)
                {
                    map[x, y].resource = Tile.tileResource.Wood;
                    if (CheckIfNextToMountain(x, y))
                    {
                        map[x, y].resource = Tile.tileResource.Stone;
                    }
                    map[x, y].amountOfResource = GenerateAmountOfResource();
                }
                else
                {
                    map[x, y].resource = Tile.tileResource.Nothing;
                    map[x, y].amountOfResource = 0;
                }

            }
        }
    }

    int GenerateAmountOfResource()
    {
        return Random.Range((int)500, (int)1000);

    }


    bool CheckIfNextToMountain(int x, int y)
    {
        for (int i = x - 1; i < x + 1; i++)
        {
            for (int j = y - 1; j < y + 1; j++)
            {
                if (i == -1 || i == MapSizeX || j == -1 || j == MapSizeY)
                {
                    continue;
                }
                else
                {
                    if (map[i, j].type == Tile.tileType.Mountain)
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
        return false;
    }

    float GetHeight(int x, int y)
    {
        float xCoords = (float)x / MapSizeX * 8;
        float yCoords = (float)y / MapSizeY * 8;
        int rand = 1;
        //int rand = Random.Range(0,10000);

        float sample = Mathf.PerlinNoise(xCoords + rand, yCoords + rand);
        return sample;
    }

    



    public float CostToEnterTile(int sourcex, int sourceY, int targetX, int targetY)
    {
        //for civ 5 cost into hills method where a unit has any movment points left
        //they can move into the hill tiles you would calculate that here
        TileType tt = tileTypes[tiles[targetX, targetY]];

        if(EnterTileCheck(targetX, targetY) == false)
        {
            return Mathf.Infinity;
        }

        float cost = tt.moveCost;

        if(sourcex != targetX && sourceY != targetY)
        {
            //diagonal moves makes diagonals cost more getting rid of stupid zig zags
            cost += 0.001f;

        }

        return cost;
    }


    void generateGraphHelp()
    {
        //initilize the graph
        graph = new Node[MapSizeX, MapSizeY];


        //initalize a spot for each node
        for (int x = 0; x < MapSizeX; x++)
        {
            for (int y = 0; y < MapSizeY; y++)
            {
                graph[x, y] = new Node();


                graph[x, y].NodeX = x;
                graph[x, y].NodeY = y;
            }
        }


        //neighbor calc
        for (int x = 0; x < MapSizeX; x++)
         {
             for (int y = 0; y < MapSizeY; y++)
              {
                

                if (y < MapSizeY - 1 && x > 0)
                {
                    graph[x, y].neighbors.Add(graph[x - 1, y +1]);  //top left
                }
                if (y < MapSizeY - 1)
                {
                    graph[x, y].neighbors.Add(graph[x, y + 1]); //top center
                }
                if (y < MapSizeY - 1 && x < MapSizeX - 1)
                {
                    graph[x, y].neighbors.Add(graph[x + 1, y + 1]);  //top right
                }
                if (x > 0)
                {
                    graph[x, y].neighbors.Add(graph[x - 1, y]); //center left
                }
                if (x < MapSizeX - 1)
                {
                    graph[x, y].neighbors.Add(graph[x + 1, y]); //center right
                }
                if (y > 0 && x > 0)
                {
                    graph[x, y].neighbors.Add(graph[x - 1, y - 1]); //bottem left
                }
                if (y > 0)
                {
                    graph[x, y].neighbors.Add(graph[x, y - 1]); //bottem center
                }
                if (y > 0 && x < MapSizeX - 1)
                {
                    graph[x, y].neighbors.Add(graph[x + 1, y - 1]);  //top left
                }



            }
        }

    }


    //creates tiles visually on the map
        void generateMapVisuals()
    {
        TileType tt = tileTypes[0];
        TileResource tr = tileResource[0];

        for (int x = 0; x < MapSizeX; x++)
        {
            for (int y = 0; y < MapSizeY; y++)
            {


                switch (map[x, y].type)
                {
                    case Tile.tileType.Grassland:
                        tt = tileTypes[0];
                        break;

                    case Tile.tileType.Marsh:
                        tt = tileTypes[1];
                        break;

                    case Tile.tileType.Mountain:
                        tt = tileTypes[2];
                        break;

                    case Tile.tileType.Water:
                        tt = tileTypes[3];
                        break;

                    default:
                        break;
                }

                        //tile type of the tiles coordinace is set so the tile can call the correct visual prefab

            GameObject go = (GameObject)Instantiate(tt.TileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);
                go.name = "Tile" + x.ToString() + "," + y.ToString();
                //the game object is initalized and created at set coordinace

                switch (map[x, y].resource)
                {
                    case Tile.tileResource.Food:
                        tr = tileResource[0];
                        break;

                    case Tile.tileResource.Stone:
                        tr = tileResource[1];
                        break;

                    case Tile.tileResource.Wood:
                        tr = tileResource[2];
                        break;

                    case Tile.tileResource.Nothing:
                        tr = tileResource[3];
                        break;

                    default:
                        break;
                }

                if (map[x, y].type != Tile.tileType.Water && map[x, y].resource != Tile.tileResource.Nothing && map[x, y].type != Tile.tileType.Mountain)
                {
                    GameObject rgo = (GameObject)Instantiate(tr.ResourceVisualPrefab, new Vector3(x, y, -.5f), Quaternion.Euler(0, 0, Random.Range(0f, 360f)), this.transform);
                    rgo.name = "Resource" + x.ToString() + "," + y.ToString();
                }

                tileClicker CT = go.GetComponent<tileClicker>();
                //gets components of the tile game object and sets them to the tile clicker to help that

                CT.Xtile = x;
                CT.Ytile = y;
                CT.map = this;
            }
        }

    }

    //get the tile coordinace on the map and relates that to the worlds coordinace  STATIC CAN BE ADDED IN TO HANDLE LARGE SCALLING OF MAP IF PROBLEMS HAPPEN REMOVE IT
    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x, y, 0);
    }





    public bool EnterTileCheck(int x, int y)
    {
        //test unit type to trerrain

        return map[x,y].isWalkable;
    }



    //sets the units data on what tile it is on and then set it up visually
    //Dijkstra's algorithm is used for the pathfinding A* search algorithm is another more commonly used pathfinging algorithm that goes in a general direction and is quicker
    //Dijkstra's was used because I did just under a year ago in COMP250
    public void MoveSelectedUnitTo(int x, int y)
    {
        GameObject selectedUnit = mouseManagerS.selectedUnit; //get the selected unit from Mouse Manager

        //clear path
        selectedUnit.GetComponent<Unit>().CurrPath = null;


        if(EnterTileCheck(x, y) == false)
        {
            //mountain click or non-move tile click
            return;
        }


        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        //setting up the unvisited nodes
        List<Node> unvisited = new List<Node>();

        Node source = graph[
                            selectedUnit.GetComponent<Unit>().Xtile, 
                            selectedUnit.GetComponent<Unit>().Ytile
                            ];

        Node target = graph[x, y];


        dist[source] = 0;
        prev[source] = null;

        //initialize everything to have an infinete distance since we don't know right now also possible that you can't reach some nodes from the source
        //which makes infinity valid
        foreach (Node v in graph)
        {
            if(v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }
            unvisited.Add(v);
        }

        while(unvisited.Count > 0)
        {
            //u is unvisited node with the smallest distance
            Node u = null;

            //helps find the unvisited node with the smallest distance
            foreach(Node PossU in unvisited)
            {
                if(u == null || dist[PossU] < dist[u])
                {
                    u = PossU;
                }
            }

            if( u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node v in u.neighbors)
            {
                //float alt = dist[u] + u.DistanceTo(v);    also NodeX = x  NodeY = y
                float alt = dist[u] + CostToEnterTile(u.NodeX, u.NodeY, v.NodeX, v.NodeY);
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        //here is shortest route found or no route found

        if(prev[target] == null)
        {
            //no route
            return;
        }

        List<Node> CurrPath = new List<Node>();
        Node curr = target;

        //flip prev list to create path
        while(curr != null)
        {
            CurrPath.Add(curr);
            curr = prev[curr];
        }

        CurrPath.Reverse();

        selectedUnit.GetComponent<Unit>().CurrPath = CurrPath;

    }

}
