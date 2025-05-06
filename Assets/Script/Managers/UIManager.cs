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

    [Header("Popup")]
    public GameObject clearPopup;
    public GameObject gameOverPopup;



    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        clearPopup.SetActive(false);
        gameOverPopup.SetActive(false);
    }

    public void UpdateLeftSpecialBlockCount(int count)
    {
        leftSpecialBlockCount.text = count.ToString();
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

    public void ShowGameClearPopup(bool show)
    {
        clearPopup.SetActive(show);
    }
    public void ShowGameOverPopup(bool show)
    {
        gameOverPopup.SetActive(show);
    }

    public void RestartGame()
    {
        GameManager.instance.RestartGame();
    }


}
