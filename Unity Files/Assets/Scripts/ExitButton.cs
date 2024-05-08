using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public void ExitSimulator()
    {
        Debug.Log("Exited!");
        //Application.Quit();
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
}
