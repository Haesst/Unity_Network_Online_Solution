using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    Transform bar;
    Image barColor;
    private void Awake()
    {
        bar = transform.Find("LifeBar");
        barColor = bar.GetComponent<Image>();
    }

    public void SetSize(float size)
    {
        if (size > 0.5f)
        {
            SetColor(Color.green);
        }
        else if(size <= 0.5f && size > 0.2f)
        {
            SetColor(Color.yellow);
        }
        else
        {
            SetColor(Color.red);
        }

        bar.localScale = new Vector2(size, 1f);
    }

    public void SetColor(Color newColor)
    {
        barColor.color = newColor;
    }
}
