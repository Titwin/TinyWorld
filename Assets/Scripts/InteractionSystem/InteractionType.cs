using UnityEngine;
using UnityEngine.EventSystems;

public class InteractionType : MonoBehaviour
{
    public enum Type
    {
        pickableBackpack,
        pickableHead,
        pickableWeapon,
        pickableSecond,
        pickableBody,
        pickableShield,

        collectWood,
        collectStone,
        collectIron,
        collectGold,
        collectCrystal,
        collectWheat,

        storeRessources,
        forge,

        construction,
        destroyBuilding,

        constructionMode, // launch construction mode from hammer on empty space
        pickableHorse,

        none              // for error code
    };
    public Type type;
    //public EventTrigger.TriggerEvent callback;

    static public bool isCollectingMinerals(InteractionType.Type type)
    {
        Type[] mineralList = { Type.collectStone, Type.collectIron, Type.collectGold, Type.collectCrystal };
        foreach (Type t in mineralList)
            if (t == type) return true;
        return false;
    }
}
