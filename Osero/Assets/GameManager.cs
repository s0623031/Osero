using UnityEngine;
using UnityEngine.UI; // UI（Slider）を制御するために必須
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // どこからでも GameManager.Instance でアクセス可能にする
    public static GameManager Instance { get; private set; }

    [Header("ゲーム設定")]
    public int MaxHP = 100;
    public int BlackHP;
    public int WhiteHP;

    [Header("UI参照")]
    // Inspectorで各プレイヤーのHPスライダーをアタッチしてください
    public Slider blackHpSlider;
    public Slider whiteHpSlider;

    // 音のストック（0:黒, 1:白）
    // Listの中身は 0=C, 1=D, ... 6=B とする
    private List<int>[] noteStocks = new List<int>[2];

    // 防御フラグ（次のターンのダメージを無効化など）
    private bool[] isGuarding = new bool[2];

    void Awake()
    {
        // シングルトンパターンの設定
        if (Instance == null)
        {
            Instance = this;
            // シーンを跨いでも破棄したくない場合は以下のコメントを外す
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 数値の初期化
        BlackHP = MaxHP;
        WhiteHP = MaxHP;
        noteStocks[0] = new List<int>();
        noteStocks[1] = new List<int>();
        isGuarding[0] = false;
        isGuarding[1] = false;
    }

    void Start()
    {
        // ゲーム開始時にUIスライダーを現在のHP（満タン）に同期させる
        UpdateHPVisuals();
    }

    // --- UI同期メソッド ---
    // スクリプト上のHP数値をスライダーの見た目に反映させる
    public void UpdateHPVisuals()
    {
        if (blackHpSlider != null)
        {
            // (float)でキャストして小数点計算を行い、0.0〜1.0の値を渡す
            blackHpSlider.value = (float)BlackHP / MaxHP;
        }
        
        if (whiteHpSlider != null)
        {
            whiteHpSlider.value = (float)WhiteHP / MaxHP;
        }
    }

    // --- ストック操作 ---
    public void AddStock(int playerIndex, int noteIndex)
    {
        if (playerIndex < 0 || playerIndex > 1) return;
        noteStocks[playerIndex].Add(noteIndex);
        Debug.Log($"Player {playerIndex} Stocked Note: {noteIndex}");
    }

    public List<int> GetStock(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 1) return new List<int>();
        return noteStocks[playerIndex];
    }

    public void ClearStock(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 1) return;
        noteStocks[playerIndex].Clear();
    }

    // --- ストック消費処理 ---
    public void ConsumeStock(int playerIndex, List<int> notesToConsume)
    {
        if (playerIndex < 0 || playerIndex > 1) return;
        foreach (int note in notesToConsume)
        {
            if (noteStocks[playerIndex].Contains(note))
            {
                noteStocks[playerIndex].Remove(note);
            }
        }
    }

    // --- バトル操作 ---
    public void ApplyDamage(int targetPlayerIndex, int damage)
    {
        // 防御判定
        if (isGuarding[targetPlayerIndex])
        {
            Debug.Log($"Player {targetPlayerIndex} 防御！ダメージ無効化");
            isGuarding[targetPlayerIndex] = false;
            return;
        }

        // 数値の計算
        if (targetPlayerIndex == 0) BlackHP -= damage;
        else WhiteHP -= damage;

        // HPの範囲制限 (0 〜 MaxHP)
        BlackHP = Mathf.Clamp(BlackHP, 0, MaxHP);
        WhiteHP = Mathf.Clamp(WhiteHP, 0, MaxHP);

        // ★計算後にUIを更新
        UpdateHPVisuals();

        // HPが0になったかチェック
        CheckGameOver();
    }

    public void Heal(int playerIndex, int amount)
    {
        if (playerIndex == 0) BlackHP += amount;
        else WhiteHP += amount;

        BlackHP = Mathf.Clamp(BlackHP, 0, MaxHP);
        WhiteHP = Mathf.Clamp(WhiteHP, 0, MaxHP);

        // ★回復後もUIを更新
        UpdateHPVisuals();
    }

    public void SetGuard(int playerIndex, bool active)
    {
        if (playerIndex < 0 || playerIndex > 1) return;
        isGuarding[playerIndex] = active;
    }

    // ゲーム終了判定
    private void CheckGameOver()
    {
        if (BlackHP <= 0)
        {
            Debug.Log("White Wins!");
            // ここにリザルト画面表示などの処理を追加
        }
        else if (WhiteHP <= 0)
        {
            Debug.Log("Black Wins!");
            // ここにリザルト画面表示などの処理を追加
        }
    }
}