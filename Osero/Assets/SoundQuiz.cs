using UnityEngine;

public class SoundQuiz : MonoBehaviour
{
    public enum Note
    {
        C, D, E, F, G, A, B
    }

    [SerializeField] public AudioClip[] pianoClips; // C, D, E, F, G, A, B の順
    public AudioSource audioSource;

    private int[] randomNoteMapping;

    // ★追加: 今回の正解の音（インデックス）を覚えておく変数
    private int currentCorrectIndex = -1; 

    void Start()
    {
        InitializeRandomMapping();
    }

    void InitializeRandomMapping()
    {
        randomNoteMapping = new int[7];
        for (int i = 0; i < 7; i++) randomNoteMapping[i] = i;

        // シャッフル
        for (int i = 0; i < randomNoteMapping.Length; i++)
        {
            int temp = randomNoteMapping[i];
            int randomIndex = Random.Range(0, randomNoteMapping.Length);
            randomNoteMapping[i] = randomNoteMapping[randomIndex];
            randomNoteMapping[randomIndex] = temp;
        }
        
        // デバッグ用ログ
        string mapLog = "今回の音階ルール: ";
        string[] noteNames = { "C", "D", "E", "F", "G", "A", "B" };
        for(int i=0; i<7; i++) mapLog += $"[{i+1}枚={noteNames[randomNoteMapping[i]]}] ";
        Debug.Log(mapLog);
    }

    // 問題の音を鳴らす（正解を確定させる）
    public void sound(int TurnCount)
    {
        if (TurnCount <= 0)
        {
            Debug.Log("返した数が不正です！");
            return;
        }

        int baseIndex = (TurnCount - 1) % 7;
        
        // ランダムマップから正解のインデックスを取得
        int randomizedIndex = randomNoteMapping[baseIndex];

        // ★ここで正解をクラス変数に保存しておく
        currentCorrectIndex = randomizedIndex;

        // 音を鳴らす
        if (randomizedIndex < pianoClips.Length)
        {
            audioSource.PlayOneShot(pianoClips[randomizedIndex]);
        }
    }

    // プレイヤーが回答ボタンを押した時の処理
    public void piano(int NoteChoiced)
    {
        // 1. 押されたボタンの音を鳴らす
        if (NoteChoiced >= 0 && NoteChoiced < pianoClips.Length)
        {
            audioSource.PlayOneShot(pianoClips[(int)NoteChoiced]);
        }

        // 2. 正誤判定
        // sound() がまだ呼ばれていない（-1）場合は判定しない安全策を入れています
        if (currentCorrectIndex == -1) 
        {
            Debug.Log("まだ出題されていません（sound関数を呼んでください）");
            return;
        }

        if (NoteChoiced == currentCorrectIndex)
        {
            Debug.Log("正解");
            // ここに正解時の処理（ストック追加など）を書く
        }
        else
        {
            Debug.Log("不正解");
            // ここに不正解時の処理（ライフ減少など）を書く
        }
    }
}