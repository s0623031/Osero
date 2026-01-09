using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ReversiManager : MonoBehaviour
{
    [Header("外部スクリプト参照")]
    public SoundQuiz soundQuiz;
    public CombinationManager combinationManager;

    [Header("画像設定")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite blackSprite;
    [SerializeField] private Sprite whiteSprite;

    [Header("UI設定")]
    [SerializeField] private Text turnText;

    // 定数
    private const int Empty = 0;
    private const int Black = 1;
    private const int White = -1;

    private int[,] board = new int[8, 8];
    private Button[] buttons;
    private int currentPlayer = Black; // 1:黒, -1:白

    // 外部から現在のターン(0 or 1)を取得するためのプロパティ
    // Black(1)なら0, White(-1)なら1を返す
    public int CurrentTurnIndex => (currentPlayer == Black) ? 0 : 1;

    // 連打防止・フェーズ管理用フラグ
    private bool isInputActive = true;

    private readonly int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
    private readonly int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        buttons = GetComponentsInChildren<Button>();

        if (buttons.Length != 64)
        {
            Debug.LogError("ボタン数が64ではありません: " + buttons.Length);
            return;
        }

        for (int i = 0; i < 64; i++)
        {
            int x = i % 8;
            int y = i / 8;
            int index = i;
            buttons[index].onClick.RemoveAllListeners();
            buttons[index].onClick.AddListener(() => OnCellClicked(x, y));
        }

        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                board[x, y] = Empty;

        board[3, 3] = White;
        board[4, 3] = Black;
        board[3, 4] = Black;
        board[4, 4] = White;

        currentPlayer = Black;
        isInputActive = true;
        UpdateBoardUI();
        UpdateTurnText();
    }

    // ボタンクリック時
    void OnCellClicked(int x, int y)
    {
        // 入力禁止中（他のフェーズ中）なら無視
        if (!isInputActive || board[x, y] != Empty) return;

        List<Vector2Int> flippableStones = GetFlippableStones(x, y, currentPlayer);

        if (flippableStones.Count > 0)
        {
            // --- 1. オセロフェーズ処理 ---
            
            // 石を置く・裏返す
            board[x, y] = currentPlayer;
            foreach (var stone in flippableStones)
            {
                board[stone.x, stone.y] = currentPlayer;
            }
            UpdateBoardUI();

            int flipCount = flippableStones.Count;
            Debug.Log($"ひっくり返した枚数: {flipCount}");

            // ★修正: ここで入力をロックする（他のフェーズが終わるまで操作不能）
            isInputActive = false;

            // --- 2. 音当てフェーズへ移行 ---
            if (soundQuiz != null)
            {
                // SoundQuizを開始
                // ※SoundQuiz側でクイズが終了したら、GoToCombinationPhaseを呼んでもらう流れ
                soundQuiz.StartQuizPhase(flipCount);
            }
            else
            {
                // SoundQuizがない場合は直でターン交代
                ProceedToNextTurn();
            }
        }
    }

    // 音当てフェーズ → 組合せフェーズへのつなぎ（SoundQuizから呼ばれる）
    public void GoToCombinationPhase()
    {
        if (combinationManager != null)
        {
            combinationManager.StartCombinationPhase();
        }
        else
        {
            ProceedToNextTurn();
        }
    }

    // --- 3. 全てのフェーズ終了後、次のターンへ進む処理 ---
    // CombinationManagerから呼ばれる
    public void ProceedToNextTurn()
    {
        // 勝敗チェック（GameManagerが存在するか確認）
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.BlackHP <= 0 || GameManager.Instance.WhiteHP <= 0)
            {
                Debug.Log("HPが0になりました。ゲーム終了");
                ShowResult(); 
                return;
            }
        }

        // プレイヤー交代
        currentPlayer = -currentPlayer;
        UpdateTurnText();
        
        // ★修正: ここで初めてロックを解除し、次のプレイヤーが石を置けるようにする
        isInputActive = true;

        // パス判定
        CheckPass();
    }

    void CheckPass()
    {
        if (!CanPlayerMove(currentPlayer))
        {
            Debug.Log($"プレイヤー {(currentPlayer == Black ? "黒" : "白")} は置ける場所がありません。パスします。");
            currentPlayer = -currentPlayer;
            UpdateTurnText();

            if (!CanPlayerMove(currentPlayer))
            {
                Debug.Log("両者置けません。ゲーム終了");
                ShowResult();
            }
        }
    }

    // --- 以下、ロジック ---

    bool CanPlayerMove(int player)
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (board[x, y] == Empty)
                    if (GetFlippableStones(x, y, player).Count > 0) return true;
            }
        }
        return false;
    }

    List<Vector2Int> GetFlippableStones(int startX, int startY, int player)
    {
        List<Vector2Int> flippable = new List<Vector2Int>();
        for (int i = 0; i < 8; i++)
        {
            List<Vector2Int> potentialFlip = new List<Vector2Int>();
            int cx = startX + dx[i];
            int cy = startY + dy[i];
            while (IsOnBoard(cx, cy) && board[cx, cy] == -player)
            {
                potentialFlip.Add(new Vector2Int(cx, cy));
                cx += dx[i];
                cy += dy[i];
            }
            if (potentialFlip.Count > 0 && IsOnBoard(cx, cy) && board[cx, cy] == player)
            {
                flippable.AddRange(potentialFlip);
            }
        }
        return flippable;
    }

    bool IsOnBoard(int x, int y) { return x >= 0 && x < 8 && y >= 0 && y < 8; }

    void UpdateBoardUI()
    {
        for (int i = 0; i < 64; i++)
        {
            int state = board[i % 8, i / 8];
            Image img = buttons[i].GetComponent<Image>();
            if (state == Empty) img.sprite = emptySprite;
            else if (state == Black) img.sprite = blackSprite;
            else if (state == White) img.sprite = whiteSprite;
        }
    }

    void UpdateTurnText()
    {
        if (turnText != null) turnText.text = (currentPlayer == Black) ? "黒の番" : "白の番";
    }

    void ShowResult()
    {
        string winner = "";

        // GameManagerがある場合のみHP判定を行う
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.WhiteHP <= 0) winner = "黒の勝ち(KO)";
            else if (GameManager.Instance.BlackHP <= 0) winner = "白の勝ち(KO)";
            else if (GameManager.Instance.BlackHP > GameManager.Instance.WhiteHP) winner = "黒の勝ち(HP判定)";
            else if (GameManager.Instance.WhiteHP > GameManager.Instance.BlackHP) winner = "白の勝ち(HP判定)";
        }

        // HP判定で勝敗が決まっていない（またはGameManagerがない）場合は枚数判定
        if (string.IsNullOrEmpty(winner))
        {
            int blackCount = 0;
            int whiteCount = 0;
            foreach (int cell in board) {
                if (cell == Black) blackCount++;
                else if (cell == White) whiteCount++;
            }
            if (blackCount > whiteCount) winner = "黒の勝ち(枚数)";
            else if (whiteCount > blackCount) winner = "白の勝ち(枚数)";
            else winner = "引き分け";
        }
        
        Debug.Log($"ゲーム終了！ {winner}");
        if (turnText != null) turnText.text = winner;
        isInputActive = false; // 操作不能にする
    }
}