using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarBehavior : MonoBehaviour
{

    public Slider slider;
    public Color low;
    public Color high;
    public Vector3 offset;

    public void SetHealth(int health, int maxHealth)
    {
        // Always true, might remove
        slider.gameObject.SetActive(health <= maxHealth);
        slider.minValue = 0f;
        slider.maxValue = (float)maxHealth;
        slider.value = (float)health;

        // slider.normalizedValue is float percentage
        // Color.Lerp - linear interpolate between colors low and high
        slider.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(low, high, slider.normalizedValue);
    }

    // Update is called once per frame
    void Update()
    {
        slider.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + offset);
    }
}
