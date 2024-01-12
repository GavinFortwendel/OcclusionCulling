using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher: MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchScene(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchScene(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchScene(2);
        }
    }

    void SwitchScene(int sceneIndex)
    {
        // Make sure the scene index is valid
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError("Invalid scene index: " + sceneIndex);
        }
    }
}