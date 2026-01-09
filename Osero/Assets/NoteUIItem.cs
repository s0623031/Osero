using UnityEngine;
using UnityEngine.UI;
using TMPro; // ★ここを追加！これがないとTMPは使えません

public class NoteUIItem : MonoBehaviour
{
    [Header("UI参照")]
    // ★ここを変更！ Text -> TextMeshProUGUI
    public TextMeshProUGUI noteText; 
    
    public Image backgroundImage;
    public Button button;

    [Header("色設定")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.cyan;

    private int noteIndex;
    private bool isSelected = false;
    private CombinationManager manager;

    public void Setup(int note, CombinationManager managerRef)
    {
        noteIndex = note;
        manager = managerRef;

        string[] noteNames = { "ド", "レ", "ミ", "ファ", "ソ", "ラ", "シ" };
        if (note >= 0 && note < noteNames.Length)
        {
            noteText.text = noteNames[note];
        }
        else
        {
            noteText.text = "?";
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
        UpdateVisual();
    }

    void OnClicked()
    {
        isSelected = !isSelected;
        UpdateVisual();
        manager.OnNoteSelectionChanged(noteIndex, isSelected);
    }

    void UpdateVisual()
    {
        backgroundImage.color = isSelected ? selectedColor : normalColor;
    }
}