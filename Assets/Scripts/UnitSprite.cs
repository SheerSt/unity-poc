using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class UnitSprite : MonoBehaviour
{

    // TODO: debug
    public Unit hitOpponent;

    private SpriteResolver spriteResolver;

    // Start is called before the first frame update
    void Start()
    {

        spriteResolver = GetComponent<SpriteResolver>();

        // Only way I could get the SetLabel event to work.
        if (spriteResolver != null) spriteResolver.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {

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

        if (hitOpponent != null) hitOpponent.animator.SetTrigger("damaged");

    }

}
