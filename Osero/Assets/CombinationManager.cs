using UnityEngine;
using UnityEngine.UI;

public class CombinationManager : MonoBehaviour
{
    [Header("参照設定")]
    public ReversiManager reversiManager; // ゲーム進行を戻すために必要
    public GameManager gameManager;       // HPデータを管理するスクリプト

    [Header("UI設定")]
    public GameObject combinationPanel;   // パネル全体（表示/非表示用）
    public Button attackButton;           // 攻撃ボタン
    public Text statusText;               // 状況表示テキスト
    public Slider blackHpSlider;          // 黒のHPバー
    public Slider whiteHpSlider;          // 白のHPバー

    // コンボダメージ（仮）
    private int pendingDamage = 0;

    void Start()
    {
        // 初期化：パネルやボタンを隠しておく
        if (combinationPanel != null) combinationPanel.SetActive(false);
        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(OnAttackButtonClicked);
            attackButton.gameObject.SetActive(false);
        }

        UpdateHpUI();
    }

    // ★ReversiManager (またはSoundQuiz) から呼ばれる
    public void StartCombinationPhase()
    {
        Debug.Log("Combination Phase Started");
        
        // パネルを表示
        if (combinationPanel != null) combinationPanel.SetActive(true);
        if (attackButton != null) attackButton.gameObject.SetActive(true);

        // 仮のダメージ計算（実際はクイズの結果などを受け取る）
        // ここでGameManagerのStockを確認してダメージを変える等の拡張が可能です
        pendingDamage = 10; 

        if (statusText != null) statusText.text = $"コンボチャンス！\n攻撃ボタンを押せ！";
    }

    // 攻撃ボタンが押されたとき
    void OnAttackButtonClicked()
    {
        // 現在のターンプレイヤーを取得（0:黒, 1:白）
        int attackerIndex = reversiManager.CurrentTurnIndex;
        int targetIndex = (attackerIndex == 0) ? 1 : 0; // 攻撃する相手（0なら1、1なら0）

        // ★修正箇所: GameManagerの新しいメソッド ApplyDamage を使用する
        gameManager.ApplyDamage(targetIndex, pendingDamage);

        // UI表示更新
        string targetName = (targetIndex == 0) ? "黒" : "白";
        if (statusText != null) statusText.text = $"{targetName}に {pendingDamage} ダメージ！";

        UpdateHpUI();

        // ボタンを隠す（連打防止）
        if (attackButton != null) attackButton.gameObject.SetActive(false);

        // 攻撃が終わったのでストックを消費する（GameManagerに追加した機能）
        gameManager.ClearStock(attackerIndex);

        // 少し待ってから次のターンへ（演出時間）
        Invoke("EndPhase", 1.5f);
    }

    void UpdateHpUI()
    {
        if (gameManager == null) return;
        
        // HPバーの更新
        if (blackHpSlider != null) 
        {
            blackHpSlider.maxValue = gameManager.MaxHP;
            blackHpSlider.value = gameManager.BlackHP;
        }
        if (whiteHpSlider != null) 
        {
            whiteHpSlider.maxValue = gameManager.MaxHP;
            whiteHpSlider.value = gameManager.WhiteHP;
        }
    }

    void EndPhase()
    {
        // パネルを閉じる
        if (combinationPanel != null) combinationPanel.SetActive(false);

        // ReversiManagerに制御を戻して、ターン交代させる
        reversiManager.ProceedToNextTurn();
    }
}