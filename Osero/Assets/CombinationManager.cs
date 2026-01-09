using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombinationManager : MonoBehaviour
{
    [Header("外部参照")]
    // PanelにあるReversiManagerをInspectorでアタッチしてください
    public ReversiManager reversiManager; 

    [Header("UI設定")]
    // NoteUIItemがアタッチされたプレハブ
    public NoteUIItem noteItemPrefab; 
    // 音アイコンを並べる親オブジェクト（Grid Layout Group付きの空オブジェクト推奨）
    public Transform stockContainer;
    // 「攻撃する」ボタン
    public Button attackButton;

    // 内部変数
    // プレイヤーが現在選択している音（ストック内のID）のリスト
    // 同じ音が複数ある場合も考慮し、単純なIntリストで管理
    private List<int> selectedNotes = new List<int>();

    // ストックUIと実際のデータの紐付け用（選択解除時にリストから削除するため）
    // Key: UIのインスタンスID, Value: 音階のIndex
    private Dictionary<int, int> activeItems = new Dictionary<int, int>();

    void Start()
    {
        // ゲーム開始時はパネル（自分自身）を非表示にする
        this.gameObject.SetActive(false);

        // ボタンにイベント登録
        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(OnAttackButtonClicked);
        }
    }

    // ReversiManagerから呼ばれる
    public void StartCombinationPhase()
    {
        Debug.Log("攻撃フェーズ開始：パネルを表示します");
        
        // 自分自身（CombinationPanel）を表示
        this.gameObject.SetActive(true);

        // 前回の選択状態をクリア
        selectedNotes.Clear();
        activeItems.Clear();

        // UI生成処理
        GenerateStockUI();
    }

    void GenerateStockUI()
    {
        // コンテナの中身（古いアイコン）を全削除
        foreach (Transform child in stockContainer)
        {
            Destroy(child.gameObject);
        }

        // GameManagerから現在のプレイヤーのストックを取得
        // ReversiManagerのCurrentTurnIndexは 0(黒) か 1(白) を返す想定
        int playerIndex = reversiManager.CurrentTurnIndex;
        List<int> currentStock = GameManager.Instance.GetStock(playerIndex);

        Debug.Log($"現在のプレイヤー: {playerIndex}, ストック数: {currentStock.Count}");

        // ストックの数だけアイコンを生成
        foreach (int noteIndex in currentStock)
        {
            NoteUIItem newItem = Instantiate(noteItemPrefab, stockContainer);
            
            // NoteUIItemのSetupを呼び出し、自分自身(this)を渡して連携する
            newItem.Setup(noteIndex, this);
        }
    }

    // NoteUIItemがクリックされた時に呼ばれる関数
    // itemScript: 呼び出し元のスクリプト, isSelected: 選択されたかどうか
    public void OnNoteSelectionChanged(int noteIndex, bool isSelected)
    {
        if (isSelected)
        {
            // 選択された音を攻撃リストに追加
            selectedNotes.Add(noteIndex);
        }
        else
        {
            // 選択解除された音を攻撃リストから削除
            // List.Removeは最初に見つかった一致する要素を1つだけ消すので、
            // 同じ音が複数あっても正しく1つだけ減ります
            selectedNotes.Remove(noteIndex);
        }

        Debug.Log($"現在の選択数: {selectedNotes.Count} (ダメージ予定: {selectedNotes.Count * 10})");
    }

    // 攻撃ボタンが押された時の処理
    void OnAttackButtonClicked()
    {
        int currentPlayerIndex = reversiManager.CurrentTurnIndex;
        // 相手のインデックス (0なら1, 1なら0)
        int targetPlayerIndex = (currentPlayerIndex == 0) ? 1 : 0;

        // 1. ダメージ計算 (1つにつき10ダメージ)
        int damage = selectedNotes.Count * 10;

        if (damage > 0)
        {
            Debug.Log($"攻撃実行！ 合計ダメージ: {damage}");
            
            // GameManagerでダメージ適用
            GameManager.Instance.ApplyDamage(targetPlayerIndex, damage);

            // GameManagerでストック消費
            GameManager.Instance.ConsumeStock(currentPlayerIndex, selectedNotes);
        }
        else
        {
            Debug.Log("音を選択せずターンを終了します（ダメージ0）");
        }

        // 処理終了
        EndCombinationPhase();
    }

    void EndCombinationPhase()
    {
        // 選択リストをクリア
        selectedNotes.Clear();

        // パネル（自分自身）を非表示にする
        this.gameObject.SetActive(false);

        // ReversiManagerにターン終了を通知
        reversiManager.ProceedToNextTurn();
    }
}