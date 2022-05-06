using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using SuperTiled2Unity;

public class Player : MonoBehaviour
{

    public float moveTime;
    public GameObject gridBox;
    public List<GameObject> gridBoxes;

    private Grid unitGrid;  // Grid that units move on.
    private bool isMoving = false;
    private Tilemap[] tileMaps;
    private BoxCollider2D boxCollider;

    // Start is called before the first frame update
    void Start()
    {

        gridBox = Instantiate(gridBox);
        gridBox.SetActive(false);

        // TODO: likely want some sort of GameManger / Game object, which keeps track the grid.
        // In that case just reference Game.instance.playerGrid.
        unitGrid = GameObject.Find("UnitGrid").GetComponent<Grid>();

        gridBoxes = new List<GameObject>();

        tileMaps = FindObjectsOfType<Tilemap>();

        boxCollider = GetComponent<BoxCollider2D>();

    }

    // Update is called once per frame
    void Update()
    {

        // If just clicked on this object, set selected = true

        // If selected, draw a box over whichever cell mouse is hovering over



        // movement coroutine - step towards the other cell


    }

    private void OnMouseDown()
    {
        // TODO: likely won't work for touch input

        if (isMoving) return;
        gridBox.SetActive(true);

        // Start showing all movement gridBoxes
        foreach (GameObject gridBox in gridBoxes)
        {
            gridBox.SetActive(true);
        }

    }

    private void OnMouseUp()
    {
        if (isMoving) return;
        gridBox.SetActive(false);

        // If mouse button just released,
        // If was on this cell, do nothing
        // If was on another cell, start movement coroutine

        // Compare the gridbox cell position to this cell position.
        Vector3Int gridboxCellPosition = unitGrid.WorldToCell(gridBox.transform.position);
        Vector3Int playerCellPosition = unitGrid.WorldToCell(transform.position);

        if (playerCellPosition != gridboxCellPosition)
        {

            StartCoroutine(Move(unitGrid.GetCellCenterWorld(gridboxCellPosition)));
            isMoving = true;

        }

    }

    private void OnMouseDrag()
    {
        if (isMoving) return;

        // Debug - draw gridBox at cursor
        Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        // Converting to cell and then back should yield something grid-aligned.
        Vector3Int cellPosition = unitGrid.WorldToCell(worldPosition);
        worldPosition = unitGrid.CellToWorld(cellPosition);

        // Make position align to 16x16 grid.
        //        worldPosition.x -= worldPosition.x % 16;
        //        worldPosition.y -= worldPosition.y % 16;
        //        worldPosition.z = 0f;  // This is a gotcha, will have z = -10 from Camera.main.ScreenToWorldPoint, which makes it invisible.

        // Set transform of gridbox
        gridBox.transform.position = worldPosition;

    }

    protected IEnumerator Move(Vector3 end)
    {

        // Destroy all of the gridboxes.
        foreach (GameObject gridBox in gridBoxes) Destroy(gridBox);

        // Get BFS path to target position.
        int maxDistance = 12;
        Vector2Int playerCellPosition = ((Vector2Int)unitGrid.WorldToCell(transform.position));
        Vector2Int targetCellPosition = ((Vector2Int)unitGrid.WorldToCell(end));
        List<Vector2Int> path = BFS(playerCellPosition, targetCellPosition, maxDistance);
        boxCollider.enabled = false;

        for (int i = 0; i < path.Count; ++i)
        {

            Vector2Int targetCell = path[i];
            Vector3 targetWorldPosition;

            if (i < path.Count - 1)
            {

                //Vector3 startWorldPosition;

                Vector2Int nextCell = path[i + 1];
                targetWorldPosition = unitGrid.GetCellCenterWorld((Vector3Int)nextCell);

                // Raycast to the cell to see if there's something solid in the way.
                // If there isn't, continue.
                // RaycastHit2D hit = Physics2D.Linecast(transform.position, targetWorldPosition);
                Vector2 direction = targetWorldPosition - transform.position;
                RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.05f, direction, direction.magnitude);
                if (hit.transform == null) continue;

            }

            targetWorldPosition = unitGrid.GetCellCenterWorld((Vector3Int)targetCell);

            // Rotate the player so that it's facing the movement direction.
            float rotationAmount = transform.rotation.eulerAngles.y;
            if (transform.position.x > targetWorldPosition.x + 0.05f) rotationAmount = 180f;
            else if (transform.position.x < targetWorldPosition.x - 0.05f) rotationAmount = 0f;
            transform.rotation = Quaternion.Euler(0, rotationAmount, 0);

            // Moves the player to a position above the cell.
            // Done that way because the Player's transform is in the middle.
            float sqrRemainingDistance = (transform.position - targetWorldPosition).sqrMagnitude;

            // Yield while moving the player.
            while (sqrRemainingDistance > float.Epsilon)
            {

                transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveTime * Time.deltaTime);
                sqrRemainingDistance = (transform.position - targetWorldPosition).sqrMagnitude;
                yield return null;

            }

        }
        boxCollider.enabled = true;


        // Done moving.
        transform.position = end;
        isMoving = false;

        // TODO: move to method
        // Re-populate the positions that the player can move to.
        playerCellPosition = ((Vector2Int)unitGrid.WorldToCell(transform.position));
        List<Vector2Int> cellPositions = BFS(playerCellPosition, Vector2Int.zero, maxDistance);

        gridBoxes.Clear();

        float sqrMaxDistance = maxDistance * maxDistance / 150f;

        foreach (Vector2Int cellPosition in cellPositions)
        {

            GameObject instance = Instantiate(gridBox);
            SpriteRenderer spriteRenderer = instance.GetComponent<SpriteRenderer>();
            instance.transform.position = unitGrid.CellToWorld((Vector3Int)cellPosition);

            // Fade alpha slightly the further the cell is from the player.
            float sqrDistance = (instance.transform.position - transform.position).sqrMagnitude;
            float alpha = sqrMaxDistance / sqrDistance;
            if (alpha > 1f) alpha = 1f;
            alpha -= 0.1f;

            spriteRenderer.color = new Color(0.5f, 0.5f, 1f, alpha);

            instance.SetActive(false);
            gridBoxes.Add(instance);

        }
    }

    /**
     * TODO: this isn't very clean.
     */
    public bool SolidTileIsHere(Vector3 worldPosition)
    {

        Vector2Int[] collisionOffsets = new Vector2Int[] { Vector2Int.zero, Vector2Int.right, new Vector2Int(1, 1), Vector2Int.up };

        // There is probably a better way to do this but there's no documentation that I can find.
        SuperTiled2Unity.CustomProperty isSolid = new SuperTiled2Unity.CustomProperty();
        isSolid.m_Name = "isSolid";
        isSolid.m_Type = "bool";
        isSolid.m_Value = "true";

        foreach (Tilemap tileMap in tileMaps)
        {
            Vector3Int cellPosition = tileMap.WorldToCell(worldPosition);

            foreach (Vector2Int collisionOffset in collisionOffsets)
            {
                // Get the tile at this position + collisionOffset.
                SuperTile superTile = tileMap.GetTile<SuperTile>(cellPosition + (Vector3Int)collisionOffset);
                if (superTile == null) continue;

                foreach (CustomProperty property in superTile.m_CustomProperties)
                {
                    if (property.m_Name.Equals(isSolid.m_Name) && property.m_Value.Equals(isSolid.m_Value)) return true;
                }
                
            }

        }

        return false;

    }

    /**
     * TODO: probably make abstract Unit, move this there.
     */
    public List<Vector2Int> BFS(Vector2Int origin, Vector2Int target, int maxDistance)
    {

        List<Vector2Int> allCells = new List<Vector2Int>();
        Queue<Vector2Int> checkThese = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> prevTiles = new Dictionary<Vector2Int, Vector2Int>();

        Vector2Int pos = new Vector2Int(origin.x, origin.y);
        Vector2Int newPos;

        checkThese.Enqueue(pos);

        Vector2Int[] offsets = new Vector2Int[] { Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up  };

        while (checkThese.Count > 0)
        {

            pos = checkThese.Dequeue();
            allCells.Add(pos);

            foreach (Vector2Int offset in offsets)
            {

                newPos = pos + offset;

                // TODO: use a distance map instead
                // Check that the tile isn't too far away.
                if (Mathf.Abs(newPos.y - origin.y) + Mathf.Abs(newPos.x - origin.x) >= maxDistance) continue;

                // Continue if there is a solid object on this cell.
                Vector3 worldPosition = unitGrid.CellToWorld((Vector3Int)newPos);
                if (SolidTileIsHere(worldPosition)) continue;

                if (!prevTiles.ContainsKey(newPos))
                {

                    checkThese.Enqueue(newPos);
                    prevTiles.Add(newPos, pos);

                }

                // If the target position is found, return the path.
                if (newPos == target)
                {

                    allCells.Clear();
                    while (newPos != origin)
                    {

                        allCells.Insert(0, newPos);
                        newPos = prevTiles[newPos];

                    }

                    return allCells;

                }

            }

        }

        return allCells;

    }

}
