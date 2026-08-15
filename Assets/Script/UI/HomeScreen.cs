using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : MonoBehaviour
{
    Button playBtn;
    void Start()
    {
        playBtn = FindFirstObjectByType<Button>();

        playBtn.onClick.AddListener(() =>
        {

        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
