using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A Scriptable Object which contains data for a player(red,blue,green,yellow) this data of player is 
///  used in creating Gameplay scene
/// </summary>
[CreateAssetMenu(fileName = "PlayerData", menuName = "Pillage/Create Player Data", order = 1)]
public class PlayerData : ScriptableObject
{
    public Sprite borderSprite;
    public GameObject warriorUIPrefab;
    public string playerName;
    public Color color;
    public Texture homeCellsTex;
    [ColorUsageAttribute(false, true)] public Color emmisionStartColor;
    [ColorUsageAttribute(false, true)] public Color emmisionColor;
    public float GlowIntensity;
    public float normalDetail;
    public Sprite playerDiceSprite;
    public SpriteState diceSpritePressed;
    // public Texture baseCellSpriteTex;
    public Texture CellTopTex;
    public WarriorType warriorType;
    public bool isBot;
    public GameObject playerFort;
    public WarriorController warriorPrefab;
    public Texture iconHomeTex;
    public List<PowerType> rumbleAvailablePowers = new List<PowerType>();
    public List<PowerType> P2AvailablePowers = new List<PowerType>();

    //Divesh - Set which  powers can player use during a game;
    public List<PowerType> availablePowers => rumbleAvailablePowers;

    public Sprite powerBotCard;

    public Sprite crownUI;

    

    [Header("Progress Bar Spirtes")]
    public Sprite sliderBackground;
    public Sprite sliderFill;


    [System.Serializable]
    public struct CastleTopData
    {
        public CastleType id;
        public Texture castleTopVfx;
    }

    public List<CastleTopData> castleTopTextures;

    [Header("Direction Arrow Spirtes")]
    public Sprite DirectionArrow;

    [Header("Selection Arrow Spirtes")]
    public Sprite SelectionArrow;



}
