using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneruleChanger : MonoBehaviour
{
    // Start画面へ戻る
    public void GoToStart()
    {
        SceneManager.LoadScene("start");
    }
}
