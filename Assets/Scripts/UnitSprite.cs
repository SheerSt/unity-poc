using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

/**
 * I tried the name 'Sprite' but that conflicted with a name in SuperTiled2Unity.
 */
public class UnitSprite : MonoBehaviour
{
    Unit unit;

    private SpriteResolver spriteResolver;

    // Start is called before the first frame update
    void Start()
    {

        spriteResolver = GetComponent<SpriteResolver>();

        // Only way I could get the SetLabel event to work.
        if (spriteResolver != null) spriteResolver.enabled = false;

        unit = transform.parent.GetComponent<Unit>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseEnter()
    {
        string template = "{0}\n\n"
                        + "  {1}/{2}\n\n"
                        + "  {3}   {4}";

        Game.manager.mouseover.SetActive(true);
        Game.manager.mouseoverText.text = string.Format(template, transform.parent.name, unit.hp, unit.maxHp, unit.attack, unit.speed);

    }

    private void OnMouseExit()
    {
        Game.manager.mouseover.SetActive(false);
    }

    private void OnMouseOver()
    {

        // Convert to Vector2 then Vector3 in order to remove the z component.
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.x += .05f;
        mousePosition.y += .15f;
        mousePosition.z = 0f;
        Game.manager.mouseover.transform.position = mousePosition;

    }


    /**
     * Very difficult to figure out.
     * 
     * Note: The spriteResolver must be disabled in order for this to work. It's disabled in Start() right now.
     */
    public void SetLabel(string label)
    {
        spriteResolver.SetCategoryAndLabel(transform.parent.name, label);

    }

    /**
     * Play a sound using the GameManager
     */
    public void PlaySound(string sound)
    {

        Game.manager.PlaySound(sound);

    }

    /**
     * This is used mid-animation to knock back the opponent.
     */
    public void HitOpponent()
    {

        if (unit.hitOpponent != null)
        {

            unit.hitOpponent.spriteAnimator.SetTrigger("damaged");
            unit.hitOpponent = null;

        }

    }

}
