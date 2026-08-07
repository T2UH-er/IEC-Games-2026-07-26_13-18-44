using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.Collections;

enum CustomerDirection
{
    Horizontal,
    Vertical
}
[System.Serializable]
public struct CustomerRequirement
{
    public int numberOfSours;
    public int numberOfSpicies;
    public int numberOfSalties;
    public int numberOfSweets;
    public int numberOfBitters;
    public int numberOfUmamis;
    public int numberOfButteries;

    CustomerRequirement(int numberOfSours = 0, int numberOfSpicies = 0, int numberOfSalties = 0, 
        int numberOfSweets = 0, int numberOfBitters = 0, int numberOfUmamis = 0, int numberOfButteries = 0)
    {
        this.numberOfSours = numberOfSours;
        this.numberOfSpicies = numberOfSpicies;
        this.numberOfSalties = numberOfSalties;
        this.numberOfSweets = numberOfSweets;
        this.numberOfBitters = numberOfBitters;
        this.numberOfUmamis = numberOfUmamis;
        this.numberOfButteries = numberOfButteries;
    }
}
public class Customer : MonoBehaviour
{
    private GridManager gridManager;
    public float offsetFromGrid = 5.5f;
    public bool isRequirementMatched = false;

    [SerializeField] private TMPro.TextMeshProUGUI requirementText;

    [SerializeField] private CustomerDirection direction;
    // Affectedzone: a list of grid cells' position that are affected by the customer, in grid coordinates
    public List<Vector2Int> affectedZone = new List<Vector2Int>();

    public CustomerRequirement requirement = new CustomerRequirement();
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridManager = GridManager.Instance;
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
                ItemData item = gridManager.GetItemAt(cell.x, cell.y);
                switch (item?.displayName)
                {
                    case "sour":
                        sour++;
                        break;
                    case "spicy":
                        spicy++;
                        break;
                    case "salty":
                        salty++;
                        break;
                    case "sweet":
                        sweet++;
                        break;
                    case "bitter":
                        bitter++;
                        break;
                    case "umami":
                        umami++;
                        break;
                    case "buttery":
                        buttery++;
                        break;
                }
            }
        }
        else
        {
            // Compare with GridManager.Instance.totalFlavorCounts
            sour = GridManager.Instance.totalFlavorCounts["sour"];
            spicy = GridManager.Instance.totalFlavorCounts["spicy"];
            salty = GridManager.Instance.totalFlavorCounts["salty"];
            sweet = GridManager.Instance.totalFlavorCounts["sweet"];
            bitter = GridManager.Instance.totalFlavorCounts["bitter"];
            umami = GridManager.Instance.totalFlavorCounts["umami"];
            buttery = GridManager.Instance.totalFlavorCounts["buttery"];

        }

        if (sour >= requirement.numberOfSours &&
            spicy >= requirement.numberOfSpicies &&
            salty >= requirement.numberOfSalties &&
            sweet >= requirement.numberOfSweets &&
            bitter >= requirement.numberOfBitters &&
            umami >= requirement.numberOfUmamis &&
            buttery >= requirement.numberOfButteries)
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
        requirementText.text = "Customer Requirement: " + requirement.numberOfSours + " sours, " + requirement.numberOfSpicies + " spicies, " + requirement.numberOfSalties + " salties, " + requirement.numberOfSweets + " sweets, " + requirement.numberOfBitters + " bitters, " + requirement.numberOfUmamis + " umamis, " + requirement.numberOfButteries + " butteries.";
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
                ItemData item = gridManager.GetItemAt(cell.x, cell.y);
                if (item != null)
                {
                    Debug.Log("Cell (" + cell.x + ", " + cell.y + "): " + item.displayName);
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
