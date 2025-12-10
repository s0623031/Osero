using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene("game");  // ←ゲームシーンの名前
        Debug.Log("a");
    }
}
