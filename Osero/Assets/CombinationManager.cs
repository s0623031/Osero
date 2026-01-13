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

    // ★重要: ストック表示を更新する関数（ReversiManagerから交代時に呼ばれる）
    public void RefreshStockDisplay()
    {
        // 既存のアイコンを削除
        foreach (Transform child in stockContainer)
        {
            Destroy(child.gameObject);
        }

        // 現在のターンのプレイヤーのストックを取得
        int playerIndex = (reversiManager.CurrentTurnIndex == 1) ? 0 : 1; // 0:黒, 1:白
        List<int> currentStock = GameManager.Instance.GetStock(playerIndex);

        foreach (int noteIndex in currentStock)
        {
            NoteUIItem newItem = Instantiate(noteItemPrefab, stockContainer);
            newItem.Setup(noteIndex, this);

            // ★ここで「攻撃フェーズ中のみ」ボタンを押せるように設定
            Button btn = newItem.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = isAttackPhase; 
            }
        }
    }

    // 攻撃フェーズ開始（SoundQuizから呼ばれる）
    public void StartCombinationPhase()
    {
        isAttackPhase = true; // ボタンを押せるようにする
        selectedNotes.Clear();
        
        if (attackButton != null) attackButton.gameObject.SetActive(true);

        // ボタンが押せる状態で再描画
        RefreshStockDisplay();
    }

    public void OnNoteSelectionChanged(int noteIndex, bool isSelected)
    {
        if (!isAttackPhase) return; // 攻撃中以外は何もしない

        if (isSelected) selectedNotes.Add(noteIndex);
        else selectedNotes.Remove(noteIndex);
    }

    void OnAttackButtonClicked()
    {
        int currentPlayerIndex = (reversiManager.CurrentTurnIndex == 1) ? 0 : 1;
        int targetPlayerIndex = (currentPlayerIndex == 0) ? 1 : 0;
        int damage = selectedNotes.Count * 10;

        if (damage > 0)
        {
            GameManager.Instance.ApplyDamage(targetPlayerIndex, damage);
            GameManager.Instance.ConsumeStock(currentPlayerIndex, selectedNotes);
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