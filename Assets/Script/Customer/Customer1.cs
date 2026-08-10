using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.Collections;

public class Customer1 : MonoBehaviour
{
    private GridManager1 gridManager;
    public float offsetFromGrid = 5.5f;
    public bool isRequirementMatched = false;

    [SerializeField] private TMPro.TextMeshProUGUI requirementText;

    [SerializeField] private CustomerDirection direction;
    // Affectedzone: a list of grid cells' position that are affected by the customer, in grid coordinates
    public List<Vector2Int> affectedZone = new List<Vector2Int>();

    public Dictionary<string, int> requirement = new Dictionary<string, int>()
    {
        { "sour", 0 },
        { "spicy", 0 },
        { "salty", 0 },
        { "sweet", 0 },
        { "bitter", 0 },
        { "umami", 0 },
        { "buttery", 0 }
    };


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridManager = GridManager1.Instance;
        requirementText.text = "";

        ConfigurePosition();
        DefaultConfigureAffectedZone();
    }
        // Update is called once per frame
    void Update()
    {
        isRequirementMatched = IsMatchRequirement();
    }

    public void ConfigurePosition()
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager is not set. Please set the GridManager in the Customer script.");
            Destroy(gameObject);
            return;
        }

        // Configure the position of the customer based on the direction and the grid size
        if (direction == CustomerDirection.Horizontal)
        {
            // The customer is placed at the upper/below side of the grid, with an offset
            Vector2Int cellPosition = gridManager.WorldToCell(transform.position);
            Debug.Log("Customer cell position: " + cellPosition);
            // if the current cell position is above the grid.
            // Let the customer's x position be the same as the cell's x position, and the y position be the cell's y position plus offsetFromGrid
            if (cellPosition.y >= gridManager.height)
            {
                transform.position = gridManager.CellToWorld(cellPosition.x, gridManager.height - 1) + new Vector3(0, offsetFromGrid, 0);
            }
            // if the current cell position is below the grid.
            else if (cellPosition.y < 0)
            {
                transform.position = gridManager.CellToWorld(cellPosition.x, 0) + new Vector3(0, -offsetFromGrid, 0);
            }
            // else, destroy the customer game object, because it is not placed correctly
            else
            {
                Debug.LogError("Customer is not placed correctly. It should be placed above or below the grid.");
                Destroy(gameObject);
            }
        }
        else if (direction == CustomerDirection.Vertical)
        {
            // The customer is placed at the left/right side of the grid, with an offset
            Vector2Int cellPosition = gridManager.WorldToCell(transform.position);
            // if the current cell position is right of the grid.
            // Let the customer's y position be the same as the cell's y position, and the x position be the cell's x position plus offsetFromGrid
            if (cellPosition.x >= gridManager.width)
            {
                transform.position = gridManager.CellToWorld(gridManager.width - 1, cellPosition.y) + new Vector3(offsetFromGrid, 0, 0);
            }
            // if the current cell position is left of the grid.
            else if (cellPosition.x < 0)
            {
                transform.position = gridManager.CellToWorld(0, cellPosition.y) + new Vector3(-offsetFromGrid, 0, 0);
            }
            // else, destroy the customer game object, because it is not placed correctly
            else
            {
                Debug.LogError("Customer is not placed correctly. It should be placed left or right of the grid.");
                Destroy(gameObject);
            }
        }
        else
        {
            // else, destroy the customer game object, because the direction is not set correctly
            Debug.LogError("Customer direction is not set correctly. It should be either Horizontal or Vertical.");
            Destroy(gameObject);
        }
    }

    // Configure the affected zone of the customer based on the direction and the grid size
    public void DefaultConfigureAffectedZone()
    {
        affectedZone.Clear();
        if (direction == CustomerDirection.Horizontal)
        {
            // The affected zone contains 6 cells in the two nearest rows (3 cells for each)
            Vector2Int cellPosition = gridManager.WorldToCell(transform.position);
            if (cellPosition.x == 0)
            {
                // The affected zone only contains 4 cells in the two nearest rows (2 cells for each)
                if (cellPosition.y >= gridManager.height)
                {
                    affectedZone.Add(new Vector2Int(0, gridManager.height - 1));
                    affectedZone.Add(new Vector2Int(1, gridManager.height - 1));
                    affectedZone.Add(new Vector2Int(0, gridManager.height - 2));
                    affectedZone.Add(new Vector2Int(1, gridManager.height - 2));
                }
                else
                {
                    affectedZone.Add(new Vector2Int(0, 0));
                    affectedZone.Add(new Vector2Int(1, 0));
                    affectedZone.Add(new Vector2Int(0, 1));
                    affectedZone.Add(new Vector2Int(1, 1));
                }
            }
            else if (cellPosition.x == gridManager.width - 1)
            {
                // The affected zone only contains 4 cells in the two nearest rows (2 cells for each)
                if (cellPosition.y >= gridManager.height)
                {
                    affectedZone.Add(new Vector2Int(gridManager.width - 1, gridManager.height - 1));
                    affectedZone.Add(new Vector2Int(gridManager.width - 2, gridManager.height - 1));
                    affectedZone.Add(new Vector2Int(gridManager.width - 1, gridManager.height - 2));
                    affectedZone.Add(new Vector2Int(gridManager.width - 2, gridManager.height - 2));
                }
                else
                {
                    affectedZone.Add(new Vector2Int(gridManager.width - 1, 0));
                    affectedZone.Add(new Vector2Int(gridManager.width - 2, 0));
                    affectedZone.Add(new Vector2Int(gridManager.width - 1, 1));
                    affectedZone.Add(new Vector2Int(gridManager.width - 2, 1));
                }
            }
            else
            {
                // The affected zone contains 6 cells in the two nearest rows (3 cells for each)
                if (cellPosition.y >= gridManager.height)
                {
                    affectedZone.Add(new Vector2Int(cellPosition.x - 1, gridManager.height - 1));
                    affectedZone.Add(new Vector2Int(cellPosition.x, gridManager.height - 1));
                    affectedZone.Add(new Vector2Int(cellPosition.x + 1, gridManager.height - 1));
                    affectedZone.Add(new Vector2Int(cellPosition.x - 1, gridManager.height - 2));
                    affectedZone.Add(new Vector2Int(cellPosition.x, gridManager.height - 2));
                    affectedZone.Add(new Vector2Int(cellPosition.x + 1, gridManager.height - 2));
                }
                else
                {
                    affectedZone.Add(new Vector2Int(cellPosition.x - 1, 0));
                    affectedZone.Add(new Vector2Int(cellPosition.x, 0));
                    affectedZone.Add(new Vector2Int(cellPosition.x + 1, 0));
                    affectedZone.Add(new Vector2Int(cellPosition.x - 1, 1));
                    affectedZone.Add(new Vector2Int(cellPosition.x, 1));
                    affectedZone.Add(new Vector2Int(cellPosition.x + 1, 1));
                }
            }
        }
        else if (direction == CustomerDirection.Vertical)
        {
            // The affected zone contains 6 cells in the two nearest rows (3 cells for each)
            Vector2Int cellPosition = gridManager.WorldToCell(transform.position);
            if (cellPosition.y == 0)
            {
                // The affected zone only contains 4 cells in the two nearest rows (2 cells for each)
                if (cellPosition.x >= gridManager.width)
                {
                    affectedZone.Add(new Vector2Int(gridManager.width - 1, 0));
                    affectedZone.Add(new Vector2Int(gridManager.width - 1, 1));
                    affectedZone.Add(new Vector2Int(gridManager.width - 2, 0));
                    affectedZone.Add(new Vector2Int(gridManager.width - 2, 1));
                }
                else
                {
                    affectedZone.Add(new Vector2Int(0, 0));
                    affectedZone.Add(new Vector2Int(0, 1));
                    affectedZone.Add(new Vector2Int(1, 0));
                    affectedZone.Add(new Vector2Int(1, 1));
                }
            }
            else if (cellPosition.y == gridManager.height - 1)
            {
                // The affected zone only contains 4 cells in the two nearest rows (2 cells for each)
                if (cellPosition.x >= gridManager.width)
                {
                    affectedZone.Add(new Vector2Int(gridManager.width - 1, gridManager.height - 1));
                    affectedZone.Add(new Vector2Int(gridManager.width - 1, gridManager.height - 2));
                    affectedZone.Add(new Vector2Int(gridManager.width - 2, gridManager.height - 1));
                    affectedZone.Add(new Vector2Int(gridManager.width - 2, gridManager.height - 2));
                }
                else
                {
                    affectedZone.Add(new Vector2Int(0, gridManager.height - 1));
                    affectedZone.Add(new Vector2Int(0, gridManager.height - 2));
                    affectedZone.Add(new Vector2Int(1, gridManager.height - 1));
                    affectedZone.Add(new Vector2Int(1, gridManager.height - 2));
                }
            }
            else
            {
                // The affected zone contains 6 cells in the two nearest rows (3 cells for each)
                if (cellPosition.x >= gridManager.width)
                {
                    affectedZone.Add(new Vector2Int(gridManager.width - 1, cellPosition.y - 1));
                    affectedZone.Add(new Vector2Int(gridManager.width - 1, cellPosition.y));
                    affectedZone.Add(new Vector2Int(gridManager.width - 1 - 1, cellPosition.y));
                    affectedZone.Add(new Vector2Int(gridManager.width - 1, cellPosition.y + 1));
                    affectedZone.Add(new Vector2Int(gridManager.width - 1 - 1, cellPosition.y + 1));
                    affectedZone.Add(new Vector2Int(gridManager.width - 1 - 1, cellPosition.y - 1));
                }

                else if (cellPosition.x < 0)
                {
                    affectedZone.Add(new Vector2Int(0, cellPosition.y - 1));
                    affectedZone.Add(new Vector2Int(0, cellPosition.y));
                    affectedZone.Add(new Vector2Int(1, cellPosition.y));
                    affectedZone.Add(new Vector2Int(0, cellPosition.y + 1));
                    affectedZone.Add(new Vector2Int(1, cellPosition.y + 1));
                    affectedZone.Add(new Vector2Int(1, cellPosition.y - 1));
                }
            }
        }
        else
        {
            // else, destroy the customer game object, because the direction is not set correctly
            Debug.LogError("Customer direction is not set correctly. It should be either Horizontal or Vertical.");
            Destroy(gameObject);
        }
    }

    public bool IsMatchRequirement() 
    {
        int sour = 0;
        int spicy = 0;
        int salty = 0;
        int sweet = 0;
        int bitter = 0;
        int umami = 0;
        int buttery = 0;
        // Kiểm tra xem các ô trong affectedZone có chứa các item yêu cầu của customer hay không
        // Nếu có, trả về true.
        if (affectedZone != null && affectedZone.Count > 0)
        {
            // Chạy đoạn mã kiểm tra các ô trong affectedZone
            foreach (Vector2Int cell in affectedZone)
            {
                ItemData1 item = gridManager.GetItemAt(cell.x, cell.y);
                foreach (var flavor in item?.flavorCounts ?? new List<FlavorData>())
                {
                    if (requirement.ContainsKey(flavor.flavorName))
                    {
                        switch (flavor.flavorName)
                        {
                            case "sour":
                                sour += flavor.count;
                                break;
                            case "spicy":
                                spicy += flavor.count;
                                break;
                            case "salty":
                                salty += flavor.count;
                                break;
                            case "sweet":
                                sweet += flavor.count;
                                break;
                            case "bitter":
                                bitter += flavor.count;
                                break;
                            case "umami":
                                umami += flavor.count;
                                break;
                            case "buttery":
                                buttery += flavor.count;
                                break;
                        }
                    }
                }
                
            }
        }
        else
        {
            // Compare with GridManager1.Instance.totalFlavorCounts
            sour = GridManager1.Instance.totalFlavorCounts["sour"];
            spicy = GridManager1.Instance.totalFlavorCounts["spicy"];
            salty = GridManager1.Instance.totalFlavorCounts["salty"];
            sweet = GridManager1.Instance.totalFlavorCounts["sweet"];
            bitter = GridManager1.Instance.totalFlavorCounts["bitter"];
            umami = GridManager1.Instance.totalFlavorCounts["umami"];
            buttery = GridManager1.Instance.totalFlavorCounts["buttery"];

        }

        if (sour >= requirement["sour"] &&
            spicy >= requirement["spicy"] &&
            salty >= requirement["salty"] &&
            sweet >= requirement["sweet"] &&
            bitter >= requirement["bitter"] &&
            umami >= requirement["umami"] &&
            buttery >= requirement["buttery"])
        {
            //Debug.Log("Customer requirement is matched.");
            return true;
        }

        // Ngược lại, trả về false.

        return false;
    }

    private void OnMouseDown()
    {
        // Khi người chơi nhấn nút, hiển thị thông tin yêu cầu của khách hàng và các item trong affectedZone.
        //Debug.Log("Customer Requirement: " + requirement.numberOfSours + " sours, " + requirement.numberOfSpicies + " spicies, " + requirement.numberOfSalties + " salties, " + requirement.numberOfSweets + " sweets, " + requirement.numberOfBitters + " bitters, " + requirement.numberOfUmamis + " umamis, " + requirement.numberOfButteries + " butteries.");
        requirementText.text = "Customer Requirement: " + requirement["sour"] + " sours, " + requirement["spicy"] + " spicies, " + requirement["salty"] + " salties, " + requirement["sweet"] + " sweets, " + requirement["bitter"] + " bitters, " + requirement["umami"] + " umamis, " + requirement["buttery"] + " butteries.";
        StartCoroutine(DisplayRequirement());
        Debug.Log("Affected Zone: ");
        if (affectedZone.Count == 0)
        {
            Debug.Log("Affected zone is empty.");
            return;
        }
        else
        {
            foreach (Vector2Int cell in affectedZone)
            {
                ItemData1 item = gridManager.GetItemAt(cell.x, cell.y);
                if (item != null)
                {
                    Debug.Log("Cell (" + cell.x + ", " + cell.y + "): " );
                    Debug.Log("  sour: " + item.GetFlavorCount("sour"));
                    Debug.Log("  spicy: " + item.GetFlavorCount("spicy"));
                    Debug.Log("  salty: " + item.GetFlavorCount("salty"));
                    Debug.Log("  sweet: " + item.GetFlavorCount("sweet"));
                    Debug.Log("  bitter: " + item.GetFlavorCount("bitter"));
                    Debug.Log("  umami: " + item.GetFlavorCount("umami"));
                    Debug.Log("  buttery: " + item.GetFlavorCount("buttery"));
                }
                else
                {
                    Debug.Log("Cell (" + cell.x + ", " + cell.y + "): empty");
                }
            }
        }
    }

    IEnumerator DisplayRequirement()
    {
        requirementText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        requirementText.gameObject.SetActive(false);
    }
}
