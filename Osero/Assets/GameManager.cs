using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("ゲーム設定")]
    public int MaxHP = 100;
    public int BlackHP;
    public int WhiteHP;

    // 音のストック（0:黒, 1:白）
    // Listの中身は 0=C, 1=D, ... 6=B とする
    private List<int>[] noteStocks = new List<int>[2];

    // 防御フラグ（次のターンのダメージを無効化など）
    private bool[] isGuarding = new bool[2];

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 初期化
        BlackHP = MaxHP;
        WhiteHP = MaxHP;
        noteStocks[0] = new List<int>();
        noteStocks[1] = new List<int>();
        isGuarding[0] = false;
        isGuarding[1] = false;
    }

    // --- ストック操作 ---
    public void AddStock(int playerIndex, int noteIndex)
    {
        noteStocks[playerIndex].Add(noteIndex);
        Debug.Log($"Player {playerIndex} Stocked Note: {noteIndex}");
    }

    public List<int> GetStock(int playerIndex)
    {
        return noteStocks[playerIndex];
    }

    public void ClearStock(int playerIndex)
    {
        noteStocks[playerIndex].Clear();
    }

    // --- バトル操作 ---
    public void ApplyDamage(int targetPlayerIndex, int damage)
    {
        // 防御判定
        if (isGuarding[targetPlayerIndex])
        {
            Debug.Log($"Player {targetPlayerIndex} 防御！ダメージ無効化");
            isGuarding[targetPlayerIndex] = false; // 防御は1回で解除などのルール
            return;
        }

        if (targetPlayerIndex == 0) BlackHP -= damage;
        else WhiteHP -= damage;

        // HPの下限
        if (BlackHP < 0) BlackHP = 0;
        if (WhiteHP < 0) WhiteHP = 0;
    }

    public void Heal(int playerIndex, int amount)
    {
        if (playerIndex == 0) BlackHP += amount;
        else WhiteHP += amount;

        if (BlackHP > MaxHP) BlackHP = MaxHP;
        if (WhiteHP > MaxHP) WhiteHP = MaxHP;
    }

    public void SetGuard(int playerIndex, bool active)
    {
        isGuarding[playerIndex] = active;
    }

    // GameManager.cs のクラス内に追加

    // --- ストック消費処理 ---
    // 指定された音（リスト）を、プレイヤーのストックから削除する
    public void ConsumeStock(int playerIndex, List<int> notesToConsume)
    {
        foreach (int note in notesToConsume)
        {
            // ストックの中にその音があれば1つだけ削除
            if (noteStocks[playerIndex].Contains(note))
            {
                noteStocks[playerIndex].Remove(note);
            }
        }
    }
    
}