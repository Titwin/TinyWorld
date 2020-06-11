using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "ScriptableTile", menuName = "Custom/ScriptableTile", order = 1)]
public class ScriptableTile : Tile
{
    [Header("Objects")]
    public GameObject ground;
    public GameObject building;
    public GameObject decoration;

    [Header("Noise")]
    public Vector2 decorationNoisePosition;
    public Vector2 decorationNoiseRotation;
    public Vector2 decorationNoiseScale = Vector2.one;

    [Header("Attributes")]
    public bool neighbourUpdate = false;
    public bool buildingUpdate = false;
    public bool isTerrain = false;
    
    [Header("Options")]
    public Material optionalMaterial;
    public Sprite optionalSprite;
    public string helperText;
}
