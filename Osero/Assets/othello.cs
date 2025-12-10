using UnityEngine;
using UnityEngine.UI;

public class othello : MonoBehaviour
{
    // 1マス分のボタンPrefab（UI Button）
    [SerializeField] GameObject cellButtonPrefab;

    // 8×8ボタンを並べる親（Grid Layout Group を付けている）
    [SerializeField] GameObject boardDisplay;

    // ボタンに割り当てる画像（Sprite）
    [SerializeField] Sprite cellSprite;

    const int WIDTH = 8;
    const int HEIGHT = 8;

    void Start()
    {
        CreateBoard();
    }

    /// <summary>
    /// 8×8 のボタンを生成し、画像を割り当てるだけ
    /// </summary>
    void CreateBoard()
    {
        // 既存のオブジェクトをクリア
        foreach (Transform child in boardDisplay.transform)
        {
            Destroy(child.gameObject);
        }

        // 8×8 生成
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                // プレハブ生成
                GameObject buttonObj = Instantiate(cellButtonPrefab);

                // 親を設定（falseでローカルスケール保持）
                buttonObj.transform.SetParent(boardDisplay.transform, false);

                // ★ ボタン画像（Image）の Sprite を割り当てる
                Image img = buttonObj.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = cellSprite;
                }

                // デバッグ用：クリックすると位置を表示
                int px = x, py = y;
                buttonObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.Log($"Clicked : {px},{py}");
                });
            }
        }
    }
}
