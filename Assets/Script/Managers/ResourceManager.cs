using System.Collections.Generic;
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
        return resourceScriptableData.blockImages[(int)color];
    }


    public List<Sprite> GetSpecialBlockImages()
    {
        return resourceScriptableData.specialBlockImages;
    }

    public Sprite GetSpecialBlockImage(int index)
    {
        index = Mathf.Clamp(index, 0, resourceScriptableData.specialBlockImages.Count - 1);
        return resourceScriptableData.specialBlockImages[index];
    }




    #endregion







}
