using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New level", menuName = "Level")]
public class Level : ScriptableObject {

    public new string name;
    public Vector2Int player_origin;
    
    public Texture2D terrain_data;
    public Texture2D foreground;
    public Texture2D background;

}
