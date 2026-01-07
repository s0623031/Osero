using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static GameManager Instance { get; private set; }

    [Header("基本設定")]
    public int MaxHP = 100;

    [Header("現在のステータス (Inspector確認用)")]
    public int BlackHP;
    public int WhiteHP;

    // ストック（音階を文字列 "C", "D", "E" などで管理）
    public List<string> BlackStock = new List<string>();
    public List<string> WhiteStock = new List<string>();

    // 簡易防御フラグ（次のダメージを1回無効化）
    public bool BlackShield = false;
    public bool WhiteShield = false;

    void Awake()
    {
        // シングルトンパターンの確立
        if (Instance == null)
        {
            Instance = this;
            // シーン遷移しても破壊されないようにする場合（必要に応じてコメントアウトを外す）
            // DontDestroyOnLoad(gameObject);
            
            // ゲーム開始時の初期化
            ResetGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ゲームの状態をリセットします（再戦時などに使用）
    /// </summary>
    public void ResetGame()
    {
        BlackHP = MaxHP;
        WhiteHP = MaxHP;
        
        BlackStock.Clear();
        WhiteStock.Clear();
        
        BlackShield = false;
        WhiteShield = false;

        Debug.Log("GameManager: ゲーム状態をリセットしました。");
    }

    // ---------------------------------------------------------
    // ストック管理
    // ---------------------------------------------------------

    /// <summary>
    /// ストックを追加します
    /// </summary>
    /// <param name="turnIndex">0:黒, 1:白</param>
    /// <param name="note">音階名 (例: "C", "D")</param>
    public void AddStock(int turnIndex, string note)
    {
        if (turnIndex == 0)
        {
            BlackStock.Add(note);
            Debug.Log($"[Stock] 黒に {note} を追加 (計: {BlackStock.Count}個)");
        }
        else
        {
            WhiteStock.Add(note);
            Debug.Log($"[Stock] 白に {note} を追加 (計: {WhiteStock.Count}個)");
        }
    }

    /// <summary>
    /// 指定したプレイヤーの全ストックを取得します
    /// </summary>
    public List<string> GetStock(int turnIndex)
    {
        return (turnIndex == 0) ? BlackStock : WhiteStock;
    }

    /// <summary>
    /// コンボ発動後などにストックを消費（クリア）します
    /// </summary>
    public void ClearStock(int turnIndex)
    {
        if (turnIndex == 0) BlackStock.Clear();
        else WhiteStock.Clear();
    }

    // ---------------------------------------------------------
    // バトル処理（ダメージ・回復・防御）
    // ---------------------------------------------------------

    /// <summary>
    /// ダメージを与えます
    /// </summary>
    /// <param name="targetTurnIndex">ダメージを受ける側 (0:黒, 1:白)</param>
    /// <param name="damage">ダメージ量</param>
    public void ApplyDamage(int targetTurnIndex, int damage)
    {
        // 黒がダメージを受ける場合
        if (targetTurnIndex == 0)
        {
            if (BlackShield)
            {
                BlackShield = false; // シールド消費
                Debug.Log("黒はシールドで攻撃を防いだ！");
                return;
            }
            BlackHP = Mathf.Max(0, BlackHP - damage);
            Debug.Log($"黒に {damage} ダメージ！ 残りHP: {BlackHP}");
        }
        // 白がダメージを受ける場合
        else
        {
            if (WhiteShield)
            {
                WhiteShield = false; // シールド消費
                Debug.Log("白はシールドで攻撃を防いだ！");
                return;
            }
            WhiteHP = Mathf.Max(0, WhiteHP - damage);
            Debug.Log($"白に {damage} ダメージ！ 残りHP: {WhiteHP}");
        }
    }

    /// <summary>
    /// HPを回復します
    /// </summary>
    public void Heal(int turnIndex, int amount)
    {
        if (turnIndex == 0)
        {
            BlackHP = Mathf.Min(BlackHP + amount, MaxHP);
            Debug.Log($"黒が {amount} 回復！ HP: {BlackHP}");
        }
        else
        {
            WhiteHP = Mathf.Min(WhiteHP + amount, MaxHP);
            Debug.Log($"白が {amount} 回復！ HP: {WhiteHP}");
        }
    }

    /// <summary>
    /// シールドを付与します
    /// </summary>
    public void AddShield(int turnIndex)
    {
        if (turnIndex == 0) BlackShield = true;
        else WhiteShield = true;
        
        Debug.Log($"{(turnIndex == 0 ? "黒" : "白")} にシールド付与！");
    }
}