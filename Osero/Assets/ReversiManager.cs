using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ReversiManager : MonoBehaviour
{
    public SoundQuiz soundQuiz;
    public int turn = 0;
    [Header("画像設定")]
    [SerializeField] private Sprite emptySprite; // 石がない時の画像
    [SerializeField] private Sprite blackSprite; // 黒石の画像
    [SerializeField] private Sprite whiteSprite; // 白石の画像

    [Header("UI設定")]
    [SerializeField] private Text turnText;      // (任意)手番を表示するテキスト

    // 盤面の状態定数
    private const int Empty = 0;
    private const int Black = 1;
    private const int White = -1;

    private int[,] board = new int[8, 8]; // 盤面データ
    private Button[] buttons;             // ボタンの配列
    private int currentPlayer = Black;    // 現在のプレイヤー（最初は黒）

    // 8方向のベクトル (上, 右上, 右, 右下, 下, 左下, 左, 左上)
    private readonly int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
    private readonly int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

    void Start()
    {
        InitializeGame();
    }

    // ゲームの初期化
    void InitializeGame()
    {
        // 子要素のボタンを全て取得 (Hierarchyの順序に依存します)
        buttons = GetComponentsInChildren<Button>();

        if (buttons.Length != 64)
        {
            Debug.LogError("ボタンの数が64個ではありません！現在: " + buttons.Length);
            return;
        }

        // ボタンにクリックイベントを登録
        for (int i = 0; i < 64; i++)
        {
            int x = i % 8;
            int y = i / 8;
            int index = i; // ローカル変数にキャプチャする

            // 既存のリスナーを削除してから追加
            buttons[index].onClick.RemoveAllListeners();
            buttons[index].onClick.AddListener(() => OnCellClicked(x, y));
        }

        // 盤面データの初期化
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                board[x, y] = Empty;
            }
        }

        // 初期配置 (オセロの配置: 中央に白黒2つずつ)
        // 配置: 左上(3,3)=白, 右上(4,3)=黒, 左下(3,4)=黒, 右下(4,4)=白
        board[3, 3] = White;
        board[4, 3] = Black;
        board[3, 4] = Black;
        board[4, 4] = White;

        currentPlayer = Black;
        UpdateBoardUI();
        UpdateTurnText();
    }

   // ボタンがクリックされた時の処理
    void OnCellClicked(int x, int y)
    {
        // 既に石がある場所には置けない
        if (board[x, y] != Empty) return;

        // 石を置けるかチェックし、裏返せる石のリストを取得
        List<Vector2Int> flippableStones = GetFlippableStones(x, y, currentPlayer);

        if (flippableStones.Count > 0)
        {
            // 1. 石を置く
            board[x, y] = currentPlayer;

            // 2. 裏返す
            foreach (var stone in flippableStones)
            {
                board[stone.x, stone.y] = currentPlayer;
            }

            // 画面更新
            UpdateBoardUI();

            // ==========================================
            // 【追加】ひっくり返した枚数を取得
            int flipCount = flippableStones.Count;

            Debug.Log($"ひっくり返した枚数: {flipCount}"); 
            soundQuiz.sound(flipCount);
            // 組合せフェーズ
            
            // GameManagerに枚数を渡して、次のフェーズ（Soundシーン）へ処理を委譲
            // ※今のところシーン移動させたくない場合はコメントアウトしてログだけ確認してください
            //GameManager.Instance.GoToSoundPhase(flipCount); 
            // ==========================================

            // プレイヤー交代
            ChangeTurn(); 
        }
        else
        {
            Debug.Log("そこには置けません");
        }
    }

    // 手番交代とパス・終了判定
    void ChangeTurn()
    {
        turn = turn + 1;
        currentPlayer = -currentPlayer; // 1 -> -1, -1 -> 1
        UpdateTurnText();

        // 次のプレイヤーが置ける場所があるか確認
        if (!CanPlayerMove(currentPlayer))
        {
            Debug.Log($"プレイヤー {(currentPlayer == Black ? "黒" : "白")} は置ける場所がありません。パスします。");
            
            // パス：相手に手番を戻す
            currentPlayer = -currentPlayer;
            UpdateTurnText();

            // 相手も置けない場合はゲーム終了
            if (!CanPlayerMove(currentPlayer))
            {
                Debug.Log("両者とも置ける場所がありません。ゲーム終了！");
                ShowResult();
            }
        }
    }

    // 指定したプレイヤーがどこかに置けるかチェック
    bool CanPlayerMove(int player)
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (board[x, y] == Empty)
                {
                    if (GetFlippableStones(x, y, player).Count > 0) return true;
                }
            }
        }
        return false;
    }

    // 指定座標に置いた場合に裏返せる石のリストを取得
    List<Vector2Int> GetFlippableStones(int startX, int startY, int player)
    {
        List<Vector2Int> flippable = new List<Vector2Int>();

        // 8方向をチェック
        for (int i = 0; i < 8; i++)
        {
            List<Vector2Int> potentialFlip = new List<Vector2Int>();
            int cx = startX + dx[i];
            int cy = startY + dy[i];

            // 相手の石が続いている間ループ
            while (IsOnBoard(cx, cy) && board[cx, cy] == -player)
            {
                potentialFlip.Add(new Vector2Int(cx, cy));
                cx += dx[i];
                cy += dy[i];
            }

            // 相手の石の先に自分の石があれば、裏返し確定
            if (potentialFlip.Count > 0 && IsOnBoard(cx, cy) && board[cx, cy] == player)
            {
                flippable.AddRange(potentialFlip);
            }
        }

        return flippable;
    }

    // 盤面内かチェック
    bool IsOnBoard(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    // UIの更新（データに基づいて画像を変更）
    void UpdateBoardUI()
    {
        for (int i = 0; i < 64; i++)
        {
            int x = i % 8;
            int y = i / 8;
            int state = board[x, y];

            Image img = buttons[i].GetComponent<Image>();
            
            if (state == Empty) img.sprite = emptySprite;
            else if (state == Black) img.sprite = blackSprite;
            else if (state == White) img.sprite = whiteSprite;
        }
    }

    // 手番表示の更新
    void UpdateTurnText()
    {
        if (turnText != null)
        {
            turnText.text = (currentPlayer == Black) ? "黒の番" : "白の番";
        }
    }

    // 結果表示（コンソールに出力）
    void ShowResult()
    {
        int blackCount = 0;
        int whiteCount = 0;

        foreach (int cell in board)
        {
            if (cell == Black) blackCount++;
            else if (cell == White) whiteCount++;
        }

        string winner;
        if (blackCount > whiteCount) winner = "黒の勝ち";
        else if (whiteCount > blackCount) winner = "白の勝ち";
        else winner = "引き分け";

        Debug.Log($"ゲーム終了！ 黒: {blackCount}, 白: {whiteCount} - {winner}");
        if (turnText != null) turnText.text = $"{winner} (黒:{blackCount} 白:{whiteCount})";
    }
}