using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance { get; private set; }

    public ResourceScriptableData resourceScriptableData;

    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    #region GetSprite


    public Sprite GetBlockImage(BlockColor color)
    {
        Sprite sprite = resourceScriptableData.blockImages.Find(x => x.name.Contains(color.ToString()));

        return sprite;
    }





    #endregion







}
