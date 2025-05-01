using UnityEngine;
using UnityEngine.UI;

public class UIHexGridGenerator : MonoBehaviour
{
    public RectTransform hexPrefab;  // 육각형 UI 오브젝트
    public int columns = 6;
    public int rows = 5;
    public float radius = 50f;  // 육각형 외접반지름 (픽셀 기준)

    void Start()
    {
        float hexWidth = 2f * radius;
        float hexHeight = Mathf.Sqrt(3) * radius;
        float xOffset = hexWidth * 0.75f;
        float yOffset = hexHeight;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                float xPos = x * xOffset;
                float yPos = y * yOffset;

                if (x % 2 == 1)
                    yPos += yOffset / 2f;  // 홀수 열은 반 칸 아래로

                RectTransform hex = Instantiate(hexPrefab, transform);
                hex.anchoredPosition = new Vector2(xPos, -yPos);  // UI 좌표 기준으로 y는 음수
                hex.name = $"Hex_{x}_{y}";
            }
        }
    }
}