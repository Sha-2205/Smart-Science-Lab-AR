using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class distanceDisplay : MonoBehaviour
{
    public TMP_Text distance;
    public GameObject ball;
    public Projectile projectile;

    void Start()
    {
        ball = GameObject.Find("ball");
        projectile = ball.GetComponent<Projectile>();
        distance.text = "Max Distance : " + projectile.maxDist.ToString("0.00");
    }
    void Update()
    {
        
    }
}
