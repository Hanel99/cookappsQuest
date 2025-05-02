using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance { get; private set; }

    public ResourceScriptableData resourceScriptableData;

    private void Awake()
    {
        instance = this;
    }


    #region GetSprite


    public Sprite GetBlockImage(BlockColor color)
    {
        Sprite sprite = resourceScriptableData.blockImages[(int)color];

        return sprite;
    }





    #endregion







}
