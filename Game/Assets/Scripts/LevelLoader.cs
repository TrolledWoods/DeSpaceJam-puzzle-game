using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour {

    public static LevelLoader instance;

    public Level[] levels;
    public int current_level = 0;

    public PlayerController player;

    // Prefabs for the interactables
    public GameObject LifePortalPrefab;
    public GameObject DeathPortalPrefab;
    public GameObject LifeFlagPole;
    public GameObject DeathFlagPole;

    // Some graphics for the level
    public Sprite[] dirt_upleft = new Sprite[8];
    public Sprite[] dirt_upright = new Sprite[8];
    public Sprite[] dirt_downleft = new Sprite[8];
    public Sprite[] dirt_downright = new Sprite[8];
    
    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start () {
        LoadLevel(current_level);
	}

    public void LoadNextLevel()
    {
        current_level++;

        if (current_level >= levels.Length)
        {
            Debug.Log("ALL LEVELS COMPLETED. TODO: Go to main menu");
            return;
        }

        LoadLevel(current_level);
    }
	
    public void ReloadLevel()
    {
        // Set the player position
        player.transform.position = new Vector3(
            levels[current_level].player_origin.x, 
            levels[current_level].player_origin.y);
        player.SetDirection(PlayerController.Direction.Right);

        // WHY don't I delete all the interactables here? Well, you see... All of the interactables aren't evil
    }

    public void LoadLevel(int lvlIndex)
    {
        current_level = lvlIndex;

        // Delete the previous level
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        
        if (Interactable.interactables != null)
        {
            while (Interactable.interactables.Count > 0)
            {
                Interactable.interactables[0].DestroyObj();
            }
        }

        bool[,] dirt_map = CreateBoolMap(levels[current_level].terrain_data, Color.black, false);

        // Create the light world
        // Get the dirt data

        // Generate the dirt tile map
        GenerateTileMap(dirt_map, dirt_upleft, dirt_upright, dirt_downleft, dirt_downright, true, 9);

            // Load all the interactables
            SummonAllCreatures(LifePortalPrefab, new Vector2(0.5f, 0.5f - 1 / 8f),
                levels[current_level].terrain_data, new Color(1f, 0, 1f), false, 9);
            SummonAllCreatures(LifeFlagPole, new Vector2(0.5f, 0.5f - 1 / 8f),
                levels[current_level].terrain_data, new Color(0f, 1f, 0f), false, 9);

        // Create the death world
        GenerateTileMap(dirt_map, dirt_upleft, dirt_upright, dirt_downleft, dirt_downright, true, 10);

            SummonAllCreatures(DeathPortalPrefab, new Vector2(0.5f, 0.5f - 1 / 8f),
                levels[current_level].terrain_data, new Color(1f, 0, 1f), false, 10);
            SummonAllCreatures(DeathFlagPole, new Vector2(0.5f, 0.5f - 1 / 8f),
                levels[current_level].terrain_data, new Color(0f, 1, 0f), false, 10);

        // Reload level loads all the temporary objects. This is a great cheezy shortcut :P
        ReloadLevel();
    }

    void SummonAllCreatures(GameObject prefab, Vector2 offset, Texture2D color_map, Color wanted_color, bool mirrored, int layer)
    {
        for (int x = 0; x < color_map.width; x++)
        {
            for (int y = 0; y < color_map.height; y++)
            {
                Debug.Log(color_map.GetPixel(x, y));
                if((color_map.GetPixel(x, y)).Equals(wanted_color))
                {
                    GameObject obj = Instantiate(prefab, new Vector2(x, y) + offset, Quaternion.identity, null);
                    obj.layer = layer;
                }
            }
        }
    }

    bool[,] CreateBoolMap(Texture2D color_map, Color required_color, bool mirrored)
    {
        bool[,] map = new bool[color_map.width, color_map.height];

        for(int x = 0; x < color_map.width; x++)
        {
            for(int y = 0; y < color_map.height; y++)
            {
                map[x, y] = (color_map.GetPixel(mirrored ? color_map.width - x - 1 : x, y)).Equals(required_color);
            }
        }

        return map;
    }

    delegate int getTile(int x, int y);

    void GenerateTileMap(bool[,] tiles, Sprite[] ul, Sprite[] ur, Sprite[] dl, Sprite[] dr, bool have_colliders, int layer)
    {
        getTile GetTile = (int x, int y) => {
            if (x < 0 || x >= tiles.GetLength(0) ||
                y < 0 || y >= tiles.GetLength(1)) return 1;
            else return tiles[x, y] ? 1 : 0;
        };

        for(int x = 0; x < tiles.GetLength(0); x++)
        {
            for(int y = 0; y < tiles.GetLength(1); y++)
            {
                if(tiles[x, y])
                {
                    // Create the tile
                    GameObject tile = new GameObject("Tile");
                    tile.transform.parent = transform;
                    tile.transform.position = new Vector3(x, y, 0);

                    if (have_colliders)
                    {
                        BoxCollider2D box = tile.AddComponent<BoxCollider2D>();
                        box.size = new Vector2(1, 1);
                        box.offset = new Vector2(0, 0);
                    }

                    int nul = GetTile(x, y + 1) + GetTile(x - 1, y + 1) * 2 + GetTile(x - 1, y) * 4;
                    int nur = GetTile(x, y + 1) + GetTile(x + 1, y + 1) * 2 + GetTile(x + 1, y) * 4;
                    int ndl = GetTile(x, y - 1) + GetTile(x - 1, y - 1) * 2 + GetTile(x - 1, y) * 4;
                    int ndr = GetTile(x, y - 1) + GetTile(x + 1, y - 1) * 2 + GetTile(x + 1, y) * 4;
                    
                    GameObject gul = new GameObject("Up-Left");
                    GameObject gur = new GameObject("Up-Right");
                    GameObject gdl = new GameObject("Down-Left");
                    GameObject gdr = new GameObject("Down-Right");

                    gul.transform.parent = tile.transform;
                    gur.transform.parent = tile.transform;
                    gdl.transform.parent = tile.transform;
                    gdr.transform.parent = tile.transform;

                    gul.transform.position = new Vector3(x - 0.25f, y + 0.25f, 0);
                    gur.transform.position = new Vector3(x + 0.25f, y + 0.25f, 0);
                    gdl.transform.position = new Vector3(x - 0.25f, y - 0.25f, 0);
                    gdr.transform.position = new Vector3(x + 0.25f, y - 0.25f, 0);

                    gul.layer = layer;
                    gur.layer = layer;
                    gdl.layer = layer;
                    gdr.layer = layer;

                    gul.AddComponent<SpriteRenderer>().sprite = ul[nul];
                    gur.AddComponent<SpriteRenderer>().sprite = ur[nur];
                    gdl.AddComponent<SpriteRenderer>().sprite = dl[ndl];
                    gdr.AddComponent<SpriteRenderer>().sprite = dr[ndr];
                }
            }
        }
    }
}
