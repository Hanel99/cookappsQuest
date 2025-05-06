using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimator : MonoBehaviour
{
    private Image image;
    public bool doAnimation = false;


    private Sprite[] sprites;
    private float duration = 0.05f;
    private Coroutine cor = null;
    private int index = 0;


    void Start()
    {
        image = GetComponent<Image>();
    }

    public void PlayAnimation()
    {
        if (sprites == null || sprites.Length == 0)
        {
            sprites = ResourceManager.instance.GetSpecialBlockImages().ToArray();
        }
        doAnimation = true;
        cor = StartCoroutine(Co_PlayAnimation());
    }
    public void StopAnimation()
    {
        doAnimation = false;
        if (cor != null)
        {
            StopCoroutine(cor);
            cor = null;
        }
    }



    IEnumerator Co_PlayAnimation()
    {
        if (sprites == null || sprites.Length == 0)
        {
            yield break;
        }

        while (doAnimation)
        {
            if (index >= sprites.Length) index = 0;
            image.sprite = sprites[index];
            yield return new WaitForSeconds(duration);

            index++;
        }
    }
}