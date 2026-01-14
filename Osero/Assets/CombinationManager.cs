using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombinationManager : MonoBehaviour
{
    [Header("外部参照")]
    public ReversiManager reversiManager;

    [Header("UI設定")]
    public NoteUIItem noteItemPrefab;
    public Transform stockContainer;
    public Button attackButton;

    private List<int> selectedNotes = new List<int>();
    private bool isAttackPhase = false; // 攻撃中かどうかのフラグ

    void Start()
    {
        // パネル自体は常に表示
        this.gameObject.SetActive(true);

        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(OnAttackButtonClicked);
            attackButton.gameObject.SetActive(false); // 攻撃ボタンは最初隠す
        }
        
        // 初回の表示更新
        RefreshStockDisplay();
    }

    // 引数なしのRefreshStockDisplayを、現在のターンに合わせて表示するように修正
    public void RefreshStockDisplay()
    {
        // ReversiManagerから現在のターン（操作権を持っているプレイヤー）を取得
        int playerIndex = reversiManager.CurrentTurnIndex; 
        UpdateStockUI(playerIndex);
    }

    // 攻撃フェーズ開始（SoundQuizから呼ばれる）
    public void StartCombinationPhase()
    {
        isAttackPhase = true; 
        selectedNotes.Clear();
        
        if (attackButton != null) attackButton.gameObject.SetActive(true);

        // ★修正：(reversiManager.CurrentTurnIndex == 1) ? 0 : 1 という計算をやめ、
        // ストック表示メソッドに「現在のプレイヤー」を直接渡すようにします。
        UpdateStockUI(reversiManager.CurrentTurnIndex);
    }

    // 混乱を避けるため名前を UpdateStockUI に変更（またはRefreshを書き換え）
    private void UpdateStockUI(int playerIndex)
    {
        // 1. 既存のアイコンを全て削除
        foreach (Transform child in stockContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. GameManagerから指定プレイヤーの所持リストを取得
        List<int> currentStock = GameManager.Instance.GetStock(playerIndex);

        // 3. プレハブを生成してリストを構築
        foreach (int noteIndex in currentStock)
        {
            NoteUIItem newItem = Instantiate(noteItemPrefab, stockContainer);
            
            // Setup内で、そのアイコンがどの音階(Index)か、及びこのマネージャーへの参照を渡す
            newItem.Setup(noteIndex, this);

            // 4. フェーズ状態に応じてボタンのクリック可否を切り替え
            Button btn = newItem.GetComponent<Button>();
            if (btn != null)
            {
                // 組み合わせフェーズ中のみ、音を選択（クリック）できる
                btn.interactable = isAttackPhase; 
            }
        }
    }

    public void OnNoteSelectionChanged(int noteIndex, bool isSelected)
    {
        // 攻撃フェーズ中以外は何もしない（念のためのガード）
        if (!isAttackPhase) return; 

        if (isSelected)
        {
            selectedNotes.Add(noteIndex);
        }
        else
        {
            selectedNotes.Remove(noteIndex);
        }
    }

    void OnAttackButtonClicked()
    {
        // 1. 今のターンのプレイヤー（石を置いた本人）が攻撃者
        int currentPlayerIndex = reversiManager.CurrentTurnIndex; 
        
        // 2. その反対側がターゲット
        int targetPlayerIndex = (currentPlayerIndex == 0) ? 1 : 0;

        int damage = selectedNotes.Count * 10;

        if (damage > 0)
        {
            // 相手にダメージを与える
            GameManager.Instance.ApplyDamage(targetPlayerIndex, damage);
            
            // ★重要：自分（currentPlayerIndex）のストックから、選んだ音を消費する
            GameManager.Instance.ConsumeStock(currentPlayerIndex, selectedNotes);
            
            Debug.Log($"Player {currentPlayerIndex} が攻撃！ Player {targetPlayerIndex} に {damage} ダメージ。自分の音を消費しました。");
        }

        EndCombinationPhase();
    }

    void EndCombinationPhase()
    {
        isAttackPhase = false; // ボタンを再び押せなくする
        if (attackButton != null) attackButton.gameObject.SetActive(false);
        
        // 次のターンの準備のためにProceedを呼ぶが、その中でRefreshStockDisplayが呼ばれる
        reversiManager.ProceedToNextTurn();
    }
}