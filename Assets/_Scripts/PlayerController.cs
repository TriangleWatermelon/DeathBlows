using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Entity
{
    #region STATS
    [SerializeField] float maxHealth;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpHeight;
    #endregion

    void Start()
    {
        health = maxHealth;
    }

    void Update()
    {
        
    }
}
