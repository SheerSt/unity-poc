using SuperTiled2Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D.Animation;

public class Unit : MonoBehaviour
{
    [HideInInspector] public GameObject gridBox;
    [HideInInspector] public Animator spriteAnimator;
    public float moveTime;
    public Unit hitOpponent;

    // Stats
    public int attack;
    public int maxHp;
    public int hp;
    public int speed;
    [HideInInspector] public SpriteResolver statusIcons;

    protected bool hasMoved = false;
    protected BoxCollider2D boxCollider;
    protected List<GameObject> gridBoxes = new List<GameObject>();
    protected List<Vector2Int> cellPositions = new List<Vector2Int>();

    private int bfsMaxDistance = 12;
    private Animator attackAnimator;
    private GameObject healthBarCanvas;
    private GameObject spriteObject;
    private GameObject statusIconsObject;
    private HealthbarBehavior healthBar;

    // Start is called before the first frame update
    protected virtual void Start()
    {

        // Keep track of necessary components.
        spriteObject = transform.Find("Sprite").gameObject;
        boxCollider = spriteObject.GetComponent<BoxCollider2D>();
        spriteAnimator = spriteObject.GetComponent<Animator>();
        Transform attackAnim = transform.Find("AttackAnim");
        if (attackAnim != null) attackAnimator = attackAnim.gameObject.GetComponent<Animator>();

        // Status icons.
        Transform statusIconTransform = transform.Find("StatusIcon");
        if (statusIconTransform != null)
        {
            statusIconsObject = statusIconTransform.gameObject;
            statusIcons = statusIconsObject.GetComponent<SpriteResolver>();

        }

        // Keep track of Health Bar.
        Transform healthBarTransform = transform.Find("HealthBar");
        if (healthBarTransform != null)
        {

            healthBarCanvas = healthBarTransform.gameObject;
            healthBar = healthBarCanvas.GetComponent<HealthbarBehavior>();
            //healthBar.transform.SetParent(transform);
            healthBar.SetHealth(hp, maxHp);

        }

        // TODO: debug, remove
        // Snap to nearest cell position
        Vector3Int cellPosition = Game.manager.unitGrid.WorldToCell(transform.position);
        transform.position = Game.manager.unitGrid.GetCellCenterWorld(cellPosition);

        // TODO: not working when called at this point for some reason,
        // hopefully switching over to use collision mesh will fix it.
        // Refresh positions that this can move to.
        if (gridBox != null) RefreshGridBoxes();

    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    /**
     * Docstring TODO.
     */
    public void ApplyDamage(int damage)
    {

        hp -= damage;
        healthBar.SetHealth(hp, maxHp);

    }

    /**
     * Moves the Unit to a given position.
     */
    public virtual IEnumerator Move(Vector3 end)
    {

        spriteAnimator.SetBool("walk", true);

        boxCollider.enabled = false;

        // Destroy all of the gridboxes.
        foreach (GameObject gridBox in gridBoxes) Destroy(gridBox);

        // Get BFS path to target position.
        Vector2Int playerCellPosition = ((Vector2Int)Game.manager.unitGrid.WorldToCell(transform.position));
        Vector2Int targetCellPosition = ((Vector2Int)Game.manager.unitGrid.WorldToCell(end));
        List<Vector2Int> path = BFS(playerCellPosition, targetCellPosition, bfsMaxDistance);

        for (int i = 0; i < path.Count; ++i)
        {

            Vector2Int targetCell = path[i];
            Vector3 targetWorldPosition;

            if (i < path.Count - 1)
            {

                Vector2Int nextCell = path[i + 1];
                targetWorldPosition = Game.manager.unitGrid.GetCellCenterWorld((Vector3Int)nextCell);

                // Raycast to the cell to see if there's something solid in the way.
                // If there isn't, continue.
                // RaycastHit2D hit = Physics2D.Linecast(transform.position, targetWorldPosition);  // <-- Worked but didn't like as much.
                Vector2 direction = targetWorldPosition - transform.position;
                RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.05f, direction, direction.magnitude);
                if (hit.transform == null) continue;

            }

            // Have the sprite look in the direction of the target world position.
            targetWorldPosition = Game.manager.unitGrid.GetCellCenterWorld((Vector3Int)targetCell);
            SpriteLookAt(targetWorldPosition);

            // If moving into an enemy, attack instead.
            Collider2D collider2D = Physics2D.OverlapPoint(targetWorldPosition);
            if (collider2D != null)
            {

                hitOpponent = collider2D.transform.parent.gameObject.GetComponent<Unit>();
                break;

            }

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
            transform.position = targetWorldPosition;

        }

        // Must yield once or spriteAnimator.SetBool() can fail if this didn't move.
        yield return null;

        // Done moving.
        //transform.position = end;  // TODO: remove
        spriteAnimator.SetBool("walk", false);

        // If hitting an opposing Unit, perform the animation.
        // `yield return` makes it so that this Coroutine won't continue until AttackAnimation() is done.
        if (hitOpponent != null) yield return StartCoroutine(AttackAnimation());

        // RefreshGridBoxes();  // TODO: remove
        boxCollider.enabled = true;

    }

    public Unit GetEnemyUnit(Vector2Int cellPosition)
    {

        Vector3 cellCenterWorldPosition = Game.manager.unitGrid.GetCellCenterWorld((Vector3Int)cellPosition);
        Collider2D hit = Physics2D.OverlapPoint(cellCenterWorldPosition);
        Unit enemyUnit = null;
        if (hit != null)
        {

            if (this is Player) enemyUnit = hit.gameObject.transform.parent.GetComponent<Enemy>();
            if (this is Enemy) enemyUnit = hit.gameObject.transform.parent.GetComponent<Player>();

        }
        return enemyUnit;

    }

    /**
     * Could consider moving this to player. Would require overriding Move().
     */
    public void RefreshGridBoxes()
    {

        // Re-populate the positions that the player can move to.
        Vector2Int playerCellPosition = ((Vector2Int)Game.manager.unitGrid.WorldToCell(transform.position));
        cellPositions.Clear();
        cellPositions.AddRange(BFS(playerCellPosition, null, bfsMaxDistance));

        gridBoxes.Clear();

        // Used to make the gridBoxes fade out away from the player.
        float sqrMaxDistance = bfsMaxDistance * bfsMaxDistance / 150f;

        // Create a new gridBox for each position in BFS.
        foreach (Vector2Int cellPosition in cellPositions)
        {

            GameObject instance = Instantiate(gridBox);
            SpriteRenderer spriteRenderer = instance.GetComponent<SpriteRenderer>();
            instance.transform.position = Game.manager.unitGrid.CellToWorld((Vector3Int)cellPosition);

            // Fade alpha slightly the further the cell is from the player.
            float sqrDistance = (instance.transform.position - transform.position).sqrMagnitude;
            float alpha = sqrMaxDistance / sqrDistance;
            if (alpha > 1f) alpha = 1f;
            alpha -= 0.1f;

            // If Enemy here, color the gridBox red.
            if (GetEnemyUnit(cellPosition) != null) spriteRenderer.color = new Color(1f, 0.25f, 0.25f, 1f);
            else spriteRenderer.color = new Color(0.5f, 0.5f, 1f, alpha);

            instance.SetActive(false);
            gridBoxes.Add(instance);

        }

    }

    /**
     * Coroutine just incase delays are needed in the future.
     */
    protected IEnumerator AttackAnimation()
    {
        healthBarCanvas.SetActive(false);

        if (Mathf.Abs(hitOpponent.transform.position.x - transform.position.x) <= float.Epsilon)
        {

            spriteAnimator.SetTrigger("attack-vertical");
            if (attackAnimator != null) attackAnimator.SetTrigger("slash-vertical");

        }
        else
        {

            spriteAnimator.SetTrigger("attack-horizontal");
            if (attackAnimator != null) attackAnimator.SetTrigger("slash-horizontal");

        }

        // Opponent looks at this unit during attack
        hitOpponent.SpriteLookAtDamaged(transform.position);

        yield return new WaitForSeconds(1.8f);

        healthBarCanvas.SetActive(true);

    }

    public virtual IEnumerator FaintAnimation()
    {

        yield return new WaitForSeconds(0.4f);

        Game.manager.PlaySound("faint4");

        yield return new WaitForSeconds(0.8f);

        Destroy(gameObject);

    }

    public bool SolidTileIsHere(Vector3 worldPosition)
    {

        return CheckforTileProperty("isSolid", worldPosition);

    }

    /**
     * TODO: tried using boxcolliders, can't get that to work. Ideally get that working at some point.
     *  - boxcolliders are working fine elsewhere, not sure why they aren't working here.
     *  - might have to do with the isTrigger variable in the collider?
     *  - or maybe the SuperTiled2Unity collision meshes / colliders are different than a boxcollider2d
     */
    public bool CheckforTileProperty(string propertyName, Vector3 worldPosition)
    {

        Vector2Int[] collisionOffsets = new Vector2Int[] { Vector2Int.zero, Vector2Int.right, new Vector2Int(1, 1), Vector2Int.up };

        // There is probably a better way to do this but there's no documentation that I can find.
        SuperTiled2Unity.CustomProperty isSolid = new SuperTiled2Unity.CustomProperty();
        isSolid.m_Name = propertyName;
        isSolid.m_Type = "bool";
        isSolid.m_Value = "true";

        foreach (Tilemap tileMap in Game.manager.tileMaps)
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
     * Rotates the Unit's sprite in order to look at the given world position.
     */
    protected void SpriteLookAt(Vector3 targetWorldPosition)
    {

        // Rotate the unit so that it's facing the movement direction.
        float yRotationAmount = transform.rotation.eulerAngles.y;
        if (transform.position.x > targetWorldPosition.x + 0.05f) yRotationAmount = 180f;
        else if (transform.position.x < targetWorldPosition.x - 0.05f) yRotationAmount = 0f;

        transform.eulerAngles = new Vector3(0f, yRotationAmount, 0f);

        float xRotationAmount = 0f;
        if (transform.position.y > targetWorldPosition.y + 0.05f) xRotationAmount = 180f;
        else if (transform.position.y < targetWorldPosition.y - 0.05f) xRotationAmount = 0f;

        transform.Rotate(xRotationAmount, 0f, 0f);

        // The very confusing thing about this is that it looks like this will
        // be set relative to this.transform when initialized (?)
        spriteObject.transform.eulerAngles = new Vector3(xRotationAmount, yRotationAmount, 0f);
        spriteObject.transform.Rotate(xRotationAmount, 0f, 0f);

        // Adjust the status icon so that it's not rotated at all.
        if (statusIconsObject != null)
        {

            statusIconsObject.transform.eulerAngles = new Vector3(xRotationAmount, yRotationAmount, 0f);
            statusIconsObject.transform.Rotate(xRotationAmount, 0f, 0f);
            //statusIconsObject.transform.Rotate(0f, -yRotationAmount, 0f);

        }

    }

    /**
     * Differs due to the damage animation. Could unify everything if damage and attack anims were the same.
     */
    protected void SpriteLookAtDamaged(Vector3 targetWorldPosition)
    {

        // Rotate the unit so that it's facing the movement direction.
        float yRotationAmount = transform.rotation.eulerAngles.y;
        if (transform.position.x > targetWorldPosition.x + 0.05f) yRotationAmount = 180f;
        else if (transform.position.x < targetWorldPosition.x - 0.05f) yRotationAmount = 0f;

        transform.eulerAngles = new Vector3(0f, yRotationAmount, 0f);

        float zRotationAmount = 0f;
        if (transform.position.y > targetWorldPosition.y + 0.05f) zRotationAmount = -90f;
        else if (transform.position.y < targetWorldPosition.y - 0.05f) zRotationAmount = 90f;

        transform.Rotate(0f, 0f, zRotationAmount);

        // The very confusing thing about this is that it looks like this will
        // be set relative to this.transform when initialized (?)
        spriteObject.transform.eulerAngles = new Vector3(0f, yRotationAmount, zRotationAmount);
        spriteObject.transform.Rotate(0f, 0f, -zRotationAmount);

        // Adjust the status icon so that it's not rotated at all.
        if (statusIconsObject != null)
        {

            statusIconsObject.transform.eulerAngles = new Vector3(0f, yRotationAmount, zRotationAmount);
            statusIconsObject.transform.Rotate(0f, 0f, -zRotationAmount);
            statusIconsObject.transform.Rotate(0f, -yRotationAmount, 0f);

        }

    }

    /**
     * If target is null, returns all non-solid tiles within maxDistance away.
     * Otherwise, returns the shortest path to target within maxDistance.
     */
    public List<Vector2Int> BFS(Vector2Int origin, Vector2Int? target, int maxDistance)
    {

        List<Vector2Int> allCells = new List<Vector2Int>();
        Queue<Vector2Int> checkThese = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int?> prevTiles = new Dictionary<Vector2Int, Vector2Int?>();
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();

        Vector2Int pos = new Vector2Int(origin.x, origin.y);
        Vector2Int newPos;

        checkThese.Enqueue(pos);
        prevTiles.Add(pos, null);
        distances.Add(pos, 0);

        Vector2Int[] offsets = new Vector2Int[] { Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };

        while (checkThese.Count > 0)
        {

            pos = checkThese.Dequeue();

            foreach (Vector2Int offset in offsets)
            {

                newPos = pos + offset;

                // Don't allow BFS to go offscreen.
                if (newPos.x < 0 || newPos.y < -16 || newPos.x > 15 || newPos.y > -1) continue;

                // TODO: use a distance map instead
                // Check that the tile isn't too far away.
                int distance = distances[pos] + 1;
                // if (Mathf.Abs(newPos.y - origin.y) + Mathf.Abs(newPos.x - origin.x) >= maxDistance) continue;  // TODO: remove
                if (distance >= maxDistance) continue;

                // Check for Enemies or Allies at this position.
                Vector3 cellCenterWorldPosition = Game.manager.unitGrid.GetCellCenterWorld((Vector3Int)newPos);
                Collider2D hit = Physics2D.OverlapPoint(cellCenterWorldPosition);
                if (hit != null)
                {

                    // Allies can't path through allies.
                    Player allyUnit = hit.gameObject.transform.parent.GetComponent<Player>();
                    if (this is Player && allyUnit != null) continue;
                    Enemy enemyUnit = hit.gameObject.transform.parent.GetComponent<Enemy>();
                    if (this is Enemy && enemyUnit != null) continue;

                }

                // Continue if there is a solid object on this cell.
                Vector3 worldPosition = Game.manager.unitGrid.CellToWorld((Vector3Int)newPos);
                if (SolidTileIsHere(worldPosition)) continue;

                if (!prevTiles.ContainsKey(newPos))
                {

                    allCells.Add(newPos);
                    prevTiles.Add(newPos, pos);
                    distances.Add(newPos, distance);
                    if (GetEnemyUnit(newPos) == null) checkThese.Enqueue(newPos);

                }

                // If the target position is found, return the path.
                if (newPos == target)
                {

                    allCells.Clear();
                    while (newPos != origin)
                    {

                        allCells.Insert(0, newPos);
                        newPos = prevTiles[newPos].Value;

                    }

                    return allCells;

                }

            }

        }

        // If there is a target position and was unable to get to it,
        // signal a path wasn't found by returning an empty List.
        if (target != null) allCells.Clear();

        return allCells;

    }

}