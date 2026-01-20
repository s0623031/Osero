using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombinationManager : MonoBehaviour
{
    [Header("外部参照")]
    public ReversiManager reversiManager;
    public SoundQuiz soundQuiz;

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
        if (attackButton != null) attackButton.interactable = false;

        int currentPlayerIndex = reversiManager.CurrentTurnIndex;
        int targetPlayerIndex = (currentPlayerIndex == 0) ? 1 : 0;

        // --- 1. 何も選択されていない時 ---
        if (selectedNotes.Count == 0)
        {
            reversiManager.SetStatus("何も選ばずにターンを終了します");
            Invoke("EndCombinationPhase", 1.0f);
            return;
        }

        selectedNotes.Sort();
        string comboKey = string.Join(",", selectedNotes);

        int damage = 0;
        int healAmount = 0;
        string message = "";
        bool shouldProceed = false;

        // --- 2. 組み合わせ判定ロジックを整理 ---
        // --- 2. 組み合わせ（コンボ）判定 ---
        // まずは特定のコンボ（3つの音など）を優先的にチェック
        switch (comboKey)
        {
            case "0,1,2,3,4,5,6": // ド・レ・ミ・ファ・ソ・ラ・シ
                damage = 100;
                message = "コンボ発動：100ダメージ！"; 
                shouldProceed = true;
                break;
            
            case "0,2,4": // ド・ミ・ソ
                damage = 50;
                message = "コンボ発動：50ダメージ！"; 
                shouldProceed = true;
                break;

            case "1,3,5": // レ・ファ・ラ
                healAmount = 50;
                message = "コンボ発動：Dm！50回復！";
                shouldProceed = true;
                break;

            case "0,2": // ド・ミ
                damage = 20;
                message = "コンボ発動：20ダメージ！";
                shouldProceed = true;
                break;
            
            case "2,4": // ミ・ソ
                damage = 20;
                message = "コンボ発動：20ダメージ！";
                shouldProceed = true;
                break;

            case "4,6": // ソ・シ
                damage = 20;
                message = "コンボ発動：20ダメージ！";
                shouldProceed = true;
                break;
            
            case "1,6": // シ・レ
                damage = 10;
                healAmount = 10;
                message = "コンボ発動：Dm！10吸収！";
                shouldProceed = true;
                break;

            case "1,3": // レ・ファ
                healAmount = 20;
                message = "コンボ発動：Dm！10回復！";
                shouldProceed = true;
                break;

            case "3,5": // ファ・ラ
                healAmount = 20;
                message = "コンボ発動：Dm！10回復！";
                shouldProceed = true;
                break;

            case "0,5": // ラ・ド
                damage = 10;
                healAmount = 10;
                message = "コンボ発動：Dm！10吸収！";
                shouldProceed = true;
                break;

            default:
            // --- 3. コンボ以外の判定 ---
            // --- 単音（どれでも1つだけ）の場合：シャッフル ---
            if (selectedNotes.Count == 1)
            {
                if (soundQuiz != null)
                {
                    soundQuiz.ShuffleMapping();
                    message = "特殊効果：音階シャッフル！";
                    shouldProceed = true;
                }
            }
            // --- それ以外（コンボ未成立など） ---
            else
            {
                message = "その組み合わせはありません";
                shouldProceed = false;
                // やり直しのためにボタンを復活
                if (attackButton != null) attackButton.interactable = true;
            }
            break;
        }

        // --- 3. 実行 ---
        reversiManager.SetStatus(message);

        if (shouldProceed)
        {
            // 成功時の処理
            if (damage > 0) GameManager.Instance.ApplyDamage(targetPlayerIndex, damage);
            if (healAmount > 0) GameManager.Instance.Heal(currentPlayerIndex, healAmount);

            GameManager.Instance.ConsumeStock(currentPlayerIndex, selectedNotes);
            
            // シャッフル時もここを通る
            Invoke("EndCombinationPhase", 1.2f);
        }
        else
        {
            // 失敗時：ボタンを復帰させる
            if (attackButton != null) attackButton.interactable = true;
        }
    }

    void EndCombinationPhase()
    {
        isAttackPhase = false; // ボタンを再び押せなくする

        if (attackButton != null)
        {
            attackButton.gameObject.SetActive(false);
            attackButton.interactable = true; // 次の自分のターンのために活性状態に戻しておく
        }

        // 次のターンの準備のためにProceedを呼ぶが、その中でRefreshStockDisplayが呼ばれる
        reversiManager.ProceedToNextTurn();
    }
}