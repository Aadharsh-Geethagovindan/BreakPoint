using UnityEngine;
using System.IO;

[System.Serializable]
public class MoveData
{
    public string name;
    public string description;
    public int cooldown;
}

[System.Serializable]
public class CharacterData
{
    public string name;
    public int hp;
    public int speed;
    public string rarity;
    public int SigChargeReq;
    public string imageName;
    public string affiliation = "";  // New
    public string species = "";      // New
    public string lore = "";         // New
    public MoveData[] moves;
}

[System.Serializable]
public class CharacterDataArray
{
    public CharacterData[] characters;
}

