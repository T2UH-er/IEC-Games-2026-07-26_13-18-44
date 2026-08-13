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

    public TextMeshProUGUI numOfFinishedText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI requirementText;

    public GameObject result;

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
        numOfFinishedText.text = "";
        resultText.text = "";
        scoreText.text = "";
        result.SetActive(false);

        // Find all customers in the scene and add them to the list
        Customer1[] customerArray = FindObjectsOfType<Customer1>();
        foreach (Customer1 customer in customerArray)
        {
            customers.Add(customer);
        }

        // Calculate the total flavor requirements for all customers
        foreach (Customer1 customer in customers)
        {
            totalFlavorReq["sour"] += customer.GetRequirementCount("sour");
            totalFlavorReq["spicy"] += customer.GetRequirementCount("spicy");
            totalFlavorReq["salty"] += customer.GetRequirementCount("salty");
            totalFlavorReq["sweet"] += customer.GetRequirementCount("sweet");
            totalFlavorReq["bitter"] += customer.GetRequirementCount("bitter");
            totalFlavorReq["umami"] += customer.GetRequirementCount("umami");
            totalFlavorReq["buttery"] += customer.GetRequirementCount("buttery");
        }

        // Log the totalFlavorReq
        //Debug.Log("Total Flavor Requirement: ");
        //Debug.Log(
        //        totalFlavorReq["sour"].ToString() + " sours, " +
        //        totalFlavorReq["spicy"].ToString() + " spicies, " +
        //        totalFlavorReq["salty"].ToString() + " salties, " +
        //        totalFlavorReq["sweet"].ToString() + " sweets, " +
        //        totalFlavorReq["bitter"].ToString() + " bitters, " +
        //        totalFlavorReq["umami"].ToString() + " umamis, " +
        //        totalFlavorReq["buttery"].ToString() + " butteries."
        //    );

        requirementText.text = "Total Requirement: " +
            totalFlavorReq["sour"] + " sours, " +
            totalFlavorReq["spicy"] + " spicies, " +
            totalFlavorReq["salty"] + " salties, " +
            totalFlavorReq["sweet"] + " sweets, " +
            totalFlavorReq["bitter"] + " bitters, " +
            totalFlavorReq["umami"] + " umamis, " +
            totalFlavorReq["buttery"] + " butteries.";
    }

    // Update is called once per frame
    void Update()
    {
        CheckWinCondition();
    }

    private int CheckWinCondition()
    {

        // count the number of finished customers
        numOfFinished = 0;
        foreach (Customer1 customer in customers)
        {
            if (customer.isRequirementMatched)
            {
                numOfFinished++;
            }
        }

        numOfFinishedText.text = "Finished Customers: " + numOfFinished + "/" + customers.Count;

        if (thresholdMoves <= 0 || availableMoves >= thresholdMoves)
        {
            scoreText.text = "Score: " + highestScore;
        }
        else
        {
            // Ép kiểu trực tiếp từ float sang int, không qua trung gian string
            int score = (int)((availableMoves * 1.0f / thresholdMoves) * highestScore);
            scoreText.text = "Score: " + score;
        }

        // return 1 if all customers are finished, return -1 if no moves left, return 0 if still in progress
        if (numOfFinished == customers.Count && availableMoves >= 0)
        {
            resultText.text = "You Win!";
            result.SetActive(true);
            // Pause the game
            Time.timeScale = 0f;
            return 1;
        }
        else if (numOfFinished < customers.Count && availableMoves <= 0)
        {
            resultText.text = "You Lose!";
            result.SetActive(true);
            // Pause the game
            Time.timeScale = 0f;
            return -1;
        }
        else
        {
            return 0;
        }
 
        
    }
        
}
