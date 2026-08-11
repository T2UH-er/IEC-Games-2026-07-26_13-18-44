using UnityEngine;

[CreateAssetMenu(fileName = "SpriteFlavor", menuName = "SpriteData/SpriteFlavor")]
public class SpriteFlavor : ScriptableObject
{
    public Sprite sour;
    public Sprite spicy;
    public Sprite salty;
    public Sprite sweet;
    public Sprite bitter;
    public Sprite umami;
    public Sprite buttery;

    public Sprite GetSprite(string flavor)
    {
        switch (flavor)
        {
            case "sour":
                return sour;
            case "spicy":
                return spicy;
            case "salty":
                return salty;
            case "sweet":
                return sweet;
            case "bitter":
                return bitter;
            case "umami":
                return umami;
            case "buttery":
                return buttery;
            default: 
                return null;
        }
                
    }    
}
