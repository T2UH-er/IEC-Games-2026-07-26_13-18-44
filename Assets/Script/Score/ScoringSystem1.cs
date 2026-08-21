using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ScoringSystem1 : MonoBehaviour
{
    public static ScoringSystem1 Instance { get; private set; }

    public Dictionary<string, int> totalFlavorReq = new Dictionary<string, int>()
    {
        { "sour", 0 },
        { "spicy", 0 },
        { "salty", 0 },
        { "sweet", 0 },
        { "bitter", 0 },
        { "umami", 0 },
        { "buttery", 0 }
    };

    private int numOfFinished = 0;
    private List<Customer1> customers = new List<Customer1>();

    public int highestScore = 0;

    public int availableMoves = 10;
    public int thresholdMoves = 10;

    //public TextMeshProUGUI numOfFinishedText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI scoreText;

    public TextMeshProUGUI availableMovesText;
    //public TextMeshProUGUI requirementText;

    public GameObject WinResult;
    public GameObject LoseResult;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Let all text be empty at the start of the game
        //if (numOfFinishedText != null) numOfFinishedText.text = "";
        if (resultText != null) resultText.text = "";
        if (scoreText != null) scoreText.text = "";
        if (WinResult != null) WinResult.SetActive(false);
        if (LoseResult != null) LoseResult.SetActive(false);

        // Fallback: Nếu trong Scene có sẵn Customer1 và chưa được spawn bởi CustomerSpawner1
        if (customers.Count == 0)
        {
            Customer1[] customerArray = FindObjectsOfType<Customer1>();
            foreach (Customer1 customer in customerArray)
            {
                customers.Add(customer);
            }
            RecalculateTotalRequirements();
        }
    }

    /// <summary>
    /// Reset lại trạng thái lượt đi, điểm số và ẩn panel kết quả khi load level mới.
    /// </summary>
    public void ResetGameStatus()
    {
        Time.timeScale = 1f;
        availableMoves = thresholdMoves;
        numOfFinished = 0;
        if (WinResult != null) WinResult.SetActive(false);
        if (LoseResult != null) LoseResult.SetActive(false);
        if (resultText != null) resultText.text = "";
        //if (numOfFinishedText != null) numOfFinishedText.text = "";
    }

    /// <summary>
    /// Khởi tạo và liên kết danh sách khách hàng được sinh ra từ CustomerSpawner1.
    /// </summary>
    public void InitializeCustomers(List<Customer1> customerList)
    {
        customers.Clear();
        if (customerList != null)
        {
            customers.AddRange(customerList);
        }
        RecalculateTotalRequirements();
        NotifyGridChanged();
    }

    private void RecalculateTotalRequirements()
    {
        // Reset về 0
        totalFlavorReq["sour"] = 0;
        totalFlavorReq["spicy"] = 0;
        totalFlavorReq["salty"] = 0;
        totalFlavorReq["sweet"] = 0;
        totalFlavorReq["bitter"] = 0;
        totalFlavorReq["umami"] = 0;
        totalFlavorReq["buttery"] = 0;

        // Calculate the total flavor requirements for all customers
        foreach (Customer1 customer in customers)
        {
            if (customer == null) continue;
            totalFlavorReq["sour"] += customer.GetRequirementCount("sour");
            totalFlavorReq["spicy"] += customer.GetRequirementCount("spicy");
            totalFlavorReq["salty"] += customer.GetRequirementCount("salty");
            totalFlavorReq["sweet"] += customer.GetRequirementCount("sweet");
            totalFlavorReq["bitter"] += customer.GetRequirementCount("bitter");
            totalFlavorReq["umami"] += customer.GetRequirementCount("umami");
            totalFlavorReq["buttery"] += customer.GetRequirementCount("buttery");
        }

        //if (requirementText != null)
        //{
        //    requirementText.text = "Total Requirement: " +
        //        totalFlavorReq["sour"] + " sours, " +
        //        totalFlavorReq["spicy"] + " spicies, " +
        //        totalFlavorReq["salty"] + " salties, " +
        //        totalFlavorReq["sweet"] + " sweets, " +
        //        totalFlavorReq["bitter"] + " bitters, " +
        //        totalFlavorReq["umami"] + " umamis, " +
        //        totalFlavorReq["buttery"] + " butteries.";
        //}
    }

    // Update chi cap nhat UI nhe, KHONG goi CheckWinCondition moi frame
    void Update()
    {
        UpdateUI();
    }

    // Cap nhat text UI (chi doc bien, khong tinh toan)
    private void UpdateUI()
    {
        //numOfFinishedText.text = "Finished Customers: " + numOfFinished + "/" + customers.Count;

        if (thresholdMoves <= 0 || availableMoves >= thresholdMoves)
            scoreText.text = "Score: " + highestScore;
        else
            scoreText.text = "Score: " + Mathf.Max(0, (int)((availableMoves * 1.0f / thresholdMoves) * highestScore));

        availableMovesText.text = availableMoves.ToString();
    }

    /// <summary>
    /// Goi sau moi lan grid thay doi (dat/boc khoi).
    /// Tinh lai tat ca customers va kiem tra win/lose.
    /// </summary>
    public void NotifyGridChanged()
    {
        // Yeu cau tung khach hang tinh lai trang thai
        numOfFinished = 0;
        foreach (Customer1 customer in customers)
        {
            customer.RefreshRequirementStatus();
            if (customer.isRequirementMatched) numOfFinished++;
        }
        CheckWinCondition();
    }

    private int CheckWinCondition()
    {
        // numOfFinished da duoc tinh trong NotifyGridChanged(), chi can doc lai
        // Bug G fix: chi xet dieu kien thang neu co it nhat 1 khach hang
        if (customers.Count > 0 && numOfFinished == customers.Count && availableMoves >= 0)
        {
            AudioManager.Instance.PlayAudio("level-complete");
            resultText.text = "You Win!";
            WinResult.SetActive(true);
            Time.timeScale = 0f;

            if (GameManager1.Instance != null)
            {
                PlayerDataManager.SaveHighestUnlockedLevel(GameManager1.Instance.currentLevelIndex);
            }

            if (GameManager1.Instance != null && GameManager1.Instance.currentLevelIndex == 30)
            {
                UnityEngine.UI.Button[] buttons = WinResult.GetComponentsInChildren<UnityEngine.UI.Button>(true);
                foreach (UnityEngine.UI.Button btn in buttons)
                {
                    if (btn.name.ToLower().Contains("next"))
                    {
                        btn.gameObject.SetActive(false);
                    }
                    else
                    {
                        TMPro.TextMeshProUGUI tmp = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                        if (tmp != null && tmp.text.ToLower().Contains("next"))
                        {
                            btn.gameObject.SetActive(false);
                        }
                    }
                }
            }

            return 1;
        }
        else if (numOfFinished < customers.Count && availableMoves <= 0)
        {
            resultText.text = "You Lose!";
            LoseResult.SetActive(true);
            Time.timeScale = 0f;
            return -1;
        }
        return 0;
    }
        
}
