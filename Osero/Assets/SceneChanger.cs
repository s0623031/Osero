using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void GoRuleScene()
    {
        SceneManager.LoadScene("rule");
    }
}
