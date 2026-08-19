using UnityEngine;
using System.Collections.Generic;
using System.Collections;
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

    [SerializeField] private int numOfFinished = 0;
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

    public GameObject chefPrefab;
    public Transform chefSpawnpoint;

    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }
    void Start()
    {

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
        gameEnded = false;
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

    void Update()
    {
        UpdateUI();
        CheckWinCondition();
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
        if (gameEnded) return 0;
        // numOfFinished da duoc tinh trong NotifyGridChanged(), chi can doc lai
        // Bug G fix: chi xet dieu kien thang neu co it nhat 1 khach hang
        if (customers.Count > 0 && numOfFinished == customers.Count && availableMoves >= 0)
        {
            gameEnded = true;
            AudioManager.Instance.PlayAudio("level-complete");
            resultText.text = "You Win!";
            //WinResult.SetActive(true);
            StartCoroutine(ShowWinBanner());
            Time.timeScale = 0f;
            return 1;
        }
        else if (numOfFinished < customers.Count && availableMoves <= 0)
        {
            gameEnded = true;
            resultText.text = "You Lose!";
            //LoseResult.SetActive(true);
            StartCoroutine(ShowLoseBanner());
            Time.timeScale = 0f;
            return -1;
        }
        return 0;
    }

    IEnumerator ShowWinBanner()
    {
        GameObject chef = Instantiate(chefPrefab, chefSpawnpoint);
        Animator chefAnimator = chef.GetComponent<Animator>();

        chefAnimator.Play("Happy");

        yield return new WaitForSecondsRealtime(3.0f);
        Destroy(chef);

        WinResult.SetActive(true);
    }

    IEnumerator ShowLoseBanner()
    {
        GameObject chef = Instantiate(chefPrefab, chefSpawnpoint);
        Animator chefAnimator = chef.GetComponent<Animator>();

        chefAnimator.Play("Cry");

        yield return new WaitForSecondsRealtime(3.0f);
        Destroy(chef);

        LoseResult.SetActive(true);
    }    
        
}
