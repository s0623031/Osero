using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject cellPrefab;

    void Start()
    {
        CreateBoard();
    }

    void CreateBoard()
    {
        for (int i = 0; i < 64; i++)
        {
            Instantiate(cellPrefab, transform);
        }
    }
}
