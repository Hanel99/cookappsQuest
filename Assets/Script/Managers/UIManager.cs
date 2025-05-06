using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }


    [Header("Special Block")]
    public Text leftSpecialBlockCount;


    [Header("MoveCount")]
    public Text leftMoveCount;

    [Header("Score")]
    public Scrollbar scoreBar;
    public Text score;



    private void Awake()
    {
        instance = this;
    }

    public void InitUI()
    {
        leftSpecialBlockCount.text = GameManager.instance.specialBlockCount.ToString();
        leftMoveCount.text = GameManager.instance.leftMoveCount.ToString();
        scoreBar.size = 0;
        score.text = GameManager.instance.score.ToString();
    }

    public void UpdateLeftMoveCount(int count)
    {
        leftMoveCount.text = count.ToString();
    }

    public void UpdateScore(int value, float barSize)
    {
        scoreBar.size = barSize;
        score.text = value.ToString();
    }




}
