﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPlayer : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = FindObjectOfType<Player>();
    }
    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            if (player.Stats.IsAlive == false)
            {
                SwoopingEvilPlatform.IsAlive = false; // Destroys swooping evil  
                Destroy(gameObject);
                player.Manager.Respawn();
            }
        }
    }
}
