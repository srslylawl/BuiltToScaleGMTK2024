using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreNumbers : MonoBehaviour
{

    byte r, g, b;

    public Vector3 scaleFactor;
    public float lifetime;
    public float posLifetimeVariance;

    private Vector3 startScale;
    private float endLifetime;

    private TextMeshProUGUI textGui;

    // Start is called before the first frame update
    void Start()
    {
        textGui = gameObject.GetComponent<TextMeshProUGUI>();

        startScale = transform.localScale;

        endLifetime = lifetime + Random.Range(0, posLifetimeVariance);
        lifetime = 0;

        byte rand = (byte)Random.Range(128, 255);

        if (textGui.text.Contains('-'))
        {
            r = 255;
            g = rand;
            b = (byte)(255 + 128 - rand);
        }
        else if(textGui.text.Contains("PERFECT"))
        {
            g = 255;
            r = rand;
            b = (byte)(255 + 128 - rand);
        }
        else
        {
            b = 255;
            r = rand;
            g = (byte)(255 + 128 - rand);
        }

        textGui.color = new Color32(r, g, b, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        lifetime += Time.deltaTime;
        Vector3 newScale = new Vector3(Mathf.Abs(scaleFactor.x * (1-Mathf.Cos((lifetime * 3f * Mathf.PI) / endLifetime))),
                                       Mathf.Abs(scaleFactor.y * (1-Mathf.Cos((lifetime * 3f * Mathf.PI) / endLifetime))),
                                       Mathf.Abs(scaleFactor.z * (1-Mathf.Cos((lifetime * 3f * Mathf.PI) / endLifetime))));
        transform.localScale = startScale + newScale;
        textGui.color = new Color(textGui.color.r, textGui.color.g, textGui.color.b, Mathf.Sin(lifetime * Mathf.PI / endLifetime));

        if (lifetime >= endLifetime) Destroy(this.gameObject);
    }
}
