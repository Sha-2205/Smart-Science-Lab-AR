using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class heightDisplay : MonoBehaviour
{
    public TMP_Text height;
    public GameObject ball;
    public Projectile projectile;

    void Start()
    {
        ball = GameObject.Find("ball");
        projectile = ball.GetComponent<Projectile>();
        height.text = "Max Height : " + projectile.maxHeight.ToString("0.00");
    }
    void Update()
    {
        
    }
}
