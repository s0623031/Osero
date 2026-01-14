using UnityEngine;

public class SoundQuiz : MonoBehaviour
{
    [Header("外部参照")]
    public ReversiManager reversiManager;
    public CombinationManager combinationManager; 
    public AudioSource audioSource;

    [Header("音源設定")]
    [SerializeField] public AudioClip[] pianoClips; // 0:ド, 1:レ... 6:シ

    private int[] randomNoteMapping;
    private int currentCorrectIndex = -1; 
    private int mistakeCount = 0;
    private bool isQuizActive = false;
    private const int MaxMistakes = 3;

    void Start() 
    { 
        InitializeRandomMapping(); 
    }

    /// <summary>
    /// ゲーム開始時に「ひっくり返した枚数」と「鳴る音」の対応表をランダムに作成
    /// </summary>
    void InitializeRandomMapping()
    {
        randomNoteMapping = new int[7];
        for (int i = 0; i < 7; i++) randomNoteMapping[i] = i;
        
        // シャッフル（フィッシャー・イェーツのシャッフル）
        for (int i = 0; i < randomNoteMapping.Length; i++) {
            int temp = randomNoteMapping[i];
            int r = Random.Range(i, randomNoteMapping.Length);
            randomNoteMapping[i] = randomNoteMapping[r];
            randomNoteMapping[r] = temp;
        }
    }

    /// <summary>
    /// オセロフェーズ終了時に呼ばれ、クイズを開始する
    /// </summary>
    /// <param name="flipCount">ひっくり返した石の枚数</param>
    public void StartQuizPhase(int flipCount)
    {
        if (flipCount <= 0) 
        {
            // 0枚（通常ありえないが）の場合は即フェーズ移行
            CallNextPhase();
            return;
        }

        mistakeCount = 0;
        isQuizActive = true;
        
        // 枚数に応じた音を決定（1枚=0番目, 2枚=1番目... 8枚=0番目...）
        int baseIndex = (flipCount - 1) % 7;
        currentCorrectIndex = randomNoteMapping[baseIndex];

        // 正解の音を鳴らす
        PlayNote(currentCorrectIndex);
        
        Debug.Log($"音当て開始! 正解音Index: {currentCorrectIndex} (枚数: {flipCount})");
    }

    /// <summary>
    /// プレイヤーが音階ボタン（ド〜シ）を押した時に呼ばれる
    /// </summary>
    /// <param name="noteChoiced">プレイヤーが選んだ音のIndex</param>
    public void OnPianoButtonClicked(int noteChoiced)
    {
        if (!isQuizActive) return;

        // 押された音を鳴らす（ヒント兼確認用）
        PlayNote(noteChoiced);

        if (noteChoiced == currentCorrectIndex)
        {
            Debug.Log("正解！ストックに追加します。");
            // GameManagerにストックを追加（現在のターンプレイヤー）
            GameManager.Instance.AddStock(reversiManager.CurrentTurnIndex, currentCorrectIndex);
            
            // ★即座にUIを更新して、手に入れた音を見せる
            if (combinationManager != null)
            {
                combinationManager.RefreshStockDisplay();
            }

            EndQuizAndProceed();
        }
        else
        {
            mistakeCount++;
            Debug.Log($"不正解！残り{MaxMistakes - mistakeCount}回");

            if (mistakeCount >= MaxMistakes)
            {
                Debug.Log("失敗... 音は獲得できませんでした。");
                EndQuizAndProceed();
            }
        }
    }

    /// <summary>
    /// クイズを終了し、少し待ってから組み合わせフェーズへ移行
    /// </summary>
    void EndQuizAndProceed()
    {
        isQuizActive = false;
        // プレイヤーが結果を認識できるよう、1秒待機してから次へ
        Invoke("CallNextPhase", 1.2f);
    }

    void CallNextPhase()
    {
        reversiManager.GoToCombinationPhase();
    }

    private void PlayNote(int index)
    {
        if (index >= 0 && index < pianoClips.Length && audioSource != null)
        {
            audioSource.PlayOneShot(pianoClips[index]);
        }
    }

    // エディタからの呼び出し用ショートカット（必要であれば）
    public void piano(int c) => OnPianoButtonClicked(c);
}