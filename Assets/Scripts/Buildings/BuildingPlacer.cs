using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    private const string CELL_NAME = "Cell";
    private const float POWER_PLANT_OFFSET_X = -0.32f;
    private const float POWER_PLANT_OFFSET_Y = -0.24f;

    public GameBoardManager board;
    public BuildingDatabase buildingDatabase;
    public LayerMask buildingLayerMask;
    public LayerMask soldierLayerMask;

    private GameObject ghost;
    private BuildingData currentBuildingData;
    private GameObject placedBuilding;
    private BuildingData selectedBuilding;
    private BaseBuilding currentSelectedBuilding;
    private bool isPlacing = false;
    private bool canPlaceCurrentBuilding = false;

    private void OnEnable()
    {
        if (UIEventDispatcher.Instance != null)
            UIEventDispatcher.Instance.OnBuildingItemClicked += OnBuildingSelected;
    }

    private void OnDisable()
    {
        if (UIEventDispatcher.Instance != null)
            UIEventDispatcher.Instance.OnBuildingItemClicked -= OnBuildingSelected;
    }

    public void OnBuildingSelected(BuildingData data, BaseBuilding instance)
    {
        if (isPlacing && currentBuildingData != null && currentBuildingData.buildingName == data.buildingName)
        {
            CancelPlacement();
            return;
        }

        if (isPlacing)
        {
            CancelPlacement();
        }

        selectedBuilding = data;
        StartPlacing(selectedBuilding);
    }

    public void StartPlacing(BuildingData buildingData)
    {
        currentBuildingData = buildingDatabase.GetBuildingByName(buildingData.buildingName);
        ghost = Instantiate(currentBuildingData.ghostPrefab);
        isPlacing = true;
        canPlaceCurrentBuilding = false;
    }

    private void CancelPlacement()
    {
        if (ghost != null)
            Destroy(ghost);
        ghost = null;
        isPlacing = false;
        canPlaceCurrentBuilding = false;
    }

    void Update()
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(1) && isPlacing)
        {
            CancelPlacement();
            return;
        }

        if (!isPlacing && Input.GetMouseButtonDown(0))
        {
            HandleBuildingSelection(mouseWorld);
            return;
        }

        if (ghost == null) return;

        HandleBuildingPlacement(mouseWorld);
    }

    private void HandleBuildingSelection(Vector2 mouseWorld)
    {
        if (IsPointerOverUI())
        {
            return;
        }

        RaycastHit2D buildingHit = Physics2D.Raycast(mouseWorld, Vector2.zero, Mathf.Infinity, buildingLayerMask);

        if (buildingHit.collider != null)
        {
            TrySelectBuilding(buildingHit);
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);
            if (hit.collider != null && !hit.collider.name.Contains(CELL_NAME))
            {
                TrySelectBuilding(hit);
            }
            else
            {
                TrySelectBuildingFromAllHits(mouseWorld);
            }
        }
    }

    private void TrySelectBuildingFromAllHits(Vector2 mouseWorld)
    {
        RaycastHit2D[] allHits = Physics2D.RaycastAll(mouseWorld, Vector2.zero);
        foreach (var rayHit in allHits)
        {
            if (!rayHit.collider.name.Contains(CELL_NAME) && rayHit.collider.GetComponent<BaseBuilding>() != null)
            {
                TrySelectBuilding(rayHit);
                return;
            }
        }
        DeselectCurrentBuilding();
    }

    private void HandleBuildingPlacement(Vector2 mouseWorld)
    {
        Vector2Int gridPos = board.WorldToGridPosition(mouseWorld);
        Vector3 basePos = board.GridToWorldPosition(gridPos.x, gridPos.y, false);
        ghost.transform.position = basePos;

        if (currentBuildingData.prefab.GetComponent<PowerPlant>())
            ghost.transform.position += new Vector3(POWER_PLANT_OFFSET_X, POWER_PLANT_OFFSET_Y, 0f);

        var baseScript = currentBuildingData.prefab.GetComponent<BaseBuilding>();
        canPlaceCurrentBuilding = board.IsAreaAvailable(gridPos.x, gridPos.y, baseScript.size.x, baseScript.size.y);
        ghost.GetComponent<SpriteRenderer>().color = canPlaceCurrentBuilding ? Color.white : Color.red;

        if (Input.GetMouseButtonDown(0) && canPlaceCurrentBuilding)
        {
            PlaceBuilding(gridPos, baseScript);
        }
    }

    private void PlaceBuilding(Vector2Int gridPos, BaseBuilding baseScript)
{
    // Defensive programming
    if (!canPlaceCurrentBuilding) return;
    
    Vector3 spawnPos = ghost.transform.position;
    
    placedBuilding = BuildingPool.Instance.GetFromPool(currentBuildingData, spawnPos, Quaternion.identity);

    var placedScript = placedBuilding.GetComponent<BaseBuilding>();
    placedScript.occupiedGridPosition = gridPos;
    placedScript.ResetBuilding();
    placedBuilding.layer = LayerMask.NameToLayer("Building");

    EnsureBuildingHasCollider(placedBuilding);
    board.OccupyArea(gridPos.x, gridPos.y, baseScript.size.x, baseScript.size.y);
    
    KillSoldiersInBuildingArea(placedScript);

    Destroy(ghost);
    ghost = null;
    isPlacing = false;
    canPlaceCurrentBuilding = false;
}
    
    private void KillSoldiersInBuildingArea(BaseBuilding building)
{
    BoxCollider2D buildingCollider = building.GetComponent<BoxCollider2D>();
    
    if (buildingCollider != null)
    {
        Vector2 colliderCenter = building.transform.position + (Vector3)buildingCollider.offset;        
        Vector2 colliderSize = buildingCollider.size;        
        colliderSize.x *= building.transform.localScale.x;
        colliderSize.y *= building.transform.localScale.y;
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(colliderCenter, colliderSize, 0, soldierLayerMask);
        
        foreach (Collider2D collider in hitColliders)
        {
            SoldierHealth soldierHealth = collider.GetComponent<SoldierHealth>();
            if (soldierHealth != null)
            {
                soldierHealth.Die();
            }
        }
    }
}

    private void EnsureBuildingHasCollider(GameObject building)
    {
        if (building.GetComponent<Collider2D>() == null)
        {
            var newCollider = building.AddComponent<BoxCollider2D>();
            var sr = building.GetComponent<SpriteRenderer>();
            if (sr != null)
                newCollider.size = sr.sprite.bounds.size;
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void TrySelectBuilding(RaycastHit2D hit)
    {
        BaseBuilding building = hit.collider.GetComponent<BaseBuilding>();
        if (building != null)
        {
            if (currentSelectedBuilding != null && currentSelectedBuilding != building)
                currentSelectedBuilding.SetSelected(false);

            building.SetSelected(true);
            currentSelectedBuilding = building;

            var infoPanel = FindObjectOfType<InfoPanelController>();
            if (infoPanel != null)
                infoPanel.OnBuildingSelected(building.buildingData, building);
        }
    }

    private void DeselectCurrentBuilding()
    {
        if (currentSelectedBuilding != null)
        {
            currentSelectedBuilding.SetSelected(false);
            currentSelectedBuilding = null;
        }
    }
    }