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

    private GameObject currentBuildingPreview;
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
        if (data == null)
        {
            return;
        }

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
        if (buildingData == null || buildingDatabase == null)
        {
            return;
        }

        currentBuildingData = buildingDatabase.GetBuildingByName(buildingData.buildingName);
        
        if (currentBuildingData == null || currentBuildingData.prefab == null)
        {
            return;
        }
        
        currentBuildingPreview = Instantiate(currentBuildingData.prefab);
        
        currentBuildingPreview.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        BaseBuilding baseBuildingScript = currentBuildingPreview.GetComponent<BaseBuilding>();
        if (baseBuildingScript != null)
        {
            baseBuildingScript.Initialize();
        }
        else
        {
            Destroy(currentBuildingPreview);
            return;
        }
        
        isPlacing = true;
        canPlaceCurrentBuilding = false;
    }

    private void CancelPlacement()
    {
        if (currentBuildingPreview != null)
            Destroy(currentBuildingPreview);
        currentBuildingPreview = null;
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

        if (currentBuildingPreview == null) return;

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

    private void TrySelectBuilding(RaycastHit2D hit)
    {
        BaseBuilding building = hit.collider.GetComponent<BaseBuilding>();
        if (building != null)
        {
            if (currentSelectedBuilding != null && currentSelectedBuilding != building)
                currentSelectedBuilding.SetSelected(false);

            currentSelectedBuilding = building;
            building.SetSelected(true);
            
            if (UIEventDispatcher.Instance != null && building.buildingData != null)
            {
                UIEventDispatcher.Instance.BuildingClicked(building.buildingData, building);
            }
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

    private void HandleBuildingPlacement(Vector2 mouseWorld)
    {
        if (board == null || currentBuildingPreview == null)
            return;

        Vector2Int gridPos = board.WorldToGridPosition(mouseWorld);
        Vector3 basePos = board.GridToWorldPosition(gridPos.x, gridPos.y, false);
        currentBuildingPreview.transform.position = basePos;

        if (currentBuildingData != null && currentBuildingData.prefab != null && 
            currentBuildingData.prefab.GetComponent<PowerPlant>())
            currentBuildingPreview.transform.position += new Vector3(POWER_PLANT_OFFSET_X, POWER_PLANT_OFFSET_Y, 0f);

        var baseScript = currentBuildingPreview.GetComponent<BaseBuilding>();
        if (baseScript != null)
        {
            canPlaceCurrentBuilding = board.IsAreaAvailable(gridPos.x, gridPos.y, baseScript.size.x, baseScript.size.y);
            
            if (baseScript.canNotPlaceIndicator != null)
            {
                baseScript.canNotPlaceIndicator.SetActive(!canPlaceCurrentBuilding);
            }

            if (Input.GetMouseButtonDown(0) && canPlaceCurrentBuilding)
            {
                PlaceBuilding(gridPos, baseScript);
            }
        }
    }

    private void PlaceBuilding(Vector2Int gridPos, BaseBuilding baseScript)
    {
        if (!canPlaceCurrentBuilding || BuildingPool.Instance == null || currentBuildingData == null) 
            return;
        
        Vector3 spawnPos = currentBuildingPreview.transform.position;
        
        placedBuilding = BuildingPool.Instance.GetFromPool(currentBuildingData, spawnPos, Quaternion.identity);

        if (placedBuilding == null)
        {
            return;
        }

        var placedScript = placedBuilding.GetComponent<BaseBuilding>();
        if (placedScript != null)
        {
            placedScript.occupiedGridPosition = gridPos;
            placedScript.ResetBuilding();
            placedBuilding.layer = LayerMask.NameToLayer("Building");

            EnsureBuildingHasCollider(placedBuilding);
            
            if (board != null && baseScript != null)
            {
                board.OccupyArea(gridPos.x, gridPos.y, baseScript.size.x, baseScript.size.y);
            }
            
            KillSoldiersInBuildingArea(placedScript);
        }

        Destroy(currentBuildingPreview);
        currentBuildingPreview = null;
        isPlacing = false;
        canPlaceCurrentBuilding = false;
    }
    
    private void KillSoldiersInBuildingArea(BaseBuilding building)
    {
        if (building == null) return;
        
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
                if (collider == null) continue;
                
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
        if (building == null) return;
        
        if (building.GetComponent<Collider2D>() == null)
        {
            var newCollider = building.AddComponent<BoxCollider2D>();
            var sr = building.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                newCollider.size = sr.sprite.bounds.size;
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}