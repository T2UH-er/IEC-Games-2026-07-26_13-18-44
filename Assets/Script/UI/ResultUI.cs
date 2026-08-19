using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultUI : MonoBehaviour
{
    [SerializeField] Button goBackBtn;

    void Start()
    {
        goBackBtn.onClick.AddListener(() => {
            AudioManager.Instance.PlayAudio("back");
            SceneManager.LoadScene("HomeScene");
        });   
    }
}
