using UnityEngine;

public class SoundQuiz : MonoBehaviour
{
    public int TurnCount = 0;
    public enum Note
    {
        C, D, E, F, G, A, B
    }
    [SerializeField] public AudioClip[] pianoClips;
    public AudioSource audioSource;
    



    public void sound(){
        if(TurnCount <= 0){
            Debug.Log("返した数が不正です！");
            return;
        }
        int note = (TurnCount - 1) % 7;
        audioSource.PlayOneShot(pianoClips[(int)note]);
        
    }
    public void piano(int NoteChoiced){
        audioSource.PlayOneShot(pianoClips[(int)NoteChoiced]);
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
