using UnityEngine;

public class SoundQuiz : MonoBehaviour
{
    public ReversiManager reversiManager;
    // CombinationManagerを直接参照するか、Findで取得するようにします
    public CombinationManager combinationManager; 
    [SerializeField] public AudioClip[] pianoClips; 
    public AudioSource audioSource;

    private int[] randomNoteMapping;
    private int currentCorrectIndex = -1; 
    private int mistakeCount = 0;
    private bool isQuizActive = false;
    private const int MaxMistakes = 3;

    void Start() { InitializeRandomMapping(); }

    void InitializeRandomMapping()
    {
        randomNoteMapping = new int[7];
        for (int i = 0; i < 7; i++) randomNoteMapping[i] = i;
        for (int i = 0; i < randomNoteMapping.Length; i++) {
            int temp = randomNoteMapping[i];
            int r = Random.Range(0, randomNoteMapping.Length);
            randomNoteMapping[i] = randomNoteMapping[r];
            randomNoteMapping[r] = temp;
        }
    }

    public void StartQuizPhase(int turnCount)
    {
        if (turnCount <= 0) return;
        mistakeCount = 0;
        isQuizActive = true;
        
        int baseIndex = (turnCount - 1) % 7;
        currentCorrectIndex = randomNoteMapping[baseIndex];

        if (currentCorrectIndex < pianoClips.Length)
            audioSource.PlayOneShot(pianoClips[currentCorrectIndex]);
        
        Debug.Log($"クイズ開始(あと{MaxMistakes}回まで) 正解Index:{currentCorrectIndex}");
    }

    public void sound(int c) => StartQuizPhase(c);

    public void piano(int NoteChoiced)
    {
        if (!isQuizActive) return;

        if (NoteChoiced >= 0 && NoteChoiced < pianoClips.Length)
            audioSource.PlayOneShot(pianoClips[NoteChoiced]);

        if (NoteChoiced == currentCorrectIndex)
        {
            Debug.Log("正解！");
            GameManager.Instance.AddStock(reversiManager.CurrentTurnIndex, currentCorrectIndex);
            EndQuizAndProceed();
        }
        else
        {
            mistakeCount++;
            Debug.Log($"不正解！残り{MaxMistakes - mistakeCount}回");
            if (mistakeCount >= MaxMistakes)
            {
                Debug.Log("失敗...");
                EndQuizAndProceed();
            }
        }
    }

    void EndQuizAndProceed()
    {
        isQuizActive = false;
        currentCorrectIndex = -1;
        
        // ★追加：クイズが終了した時点でストックのUI表示を更新する
        if (combinationManager != null)
        {
            combinationManager.RefreshStockDisplay();
        }
        else
        {
            // 設定されていない場合の予備（最新のメソッドに書き換え）
            Object.FindFirstObjectByType<CombinationManager>()?.RefreshStockDisplay();
        }

        Invoke("CallNextPhase", 1.0f);
    }

    void CallNextPhase()
    {
        reversiManager.GoToCombinationPhase();
    }
}