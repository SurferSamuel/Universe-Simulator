using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public Animator animator;

    public void Clicked()
    {
        animator.SetTrigger("FadeOut");
    }

    public void OnFadeComplete()
    {
        int newSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(newSceneIndex);
    }
}
