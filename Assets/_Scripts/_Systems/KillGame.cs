using UnityEngine;

public class KillGame : MonoBehaviour
{
    public void ExitGame()
    {
        Application.Quit();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) 
            ExitGame();
    }
}
