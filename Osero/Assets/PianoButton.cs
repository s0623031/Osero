using UnityEngine;

public class PianoButton : MonoBehaviour
{
    public SoundQuiz soundQuiz;
    public int ScaleNo;

    public void OnPianoClicked(){
        int NoteChoiced = (ScaleNo - 1) % 7;
        soundQuiz.piano(NoteChoiced);
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
