using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sniper : Unit {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    public void Initialize(TileCoord pos, Team team)
    {
        List<TileCoord> displacementsForShape = new List<TileCoord>();
        displacementsForShape.Add(new TileCoord(0, 0));
        Initialize(pos, 4, '>', team, 1f, 3, 1, 6, 3, 0, 4, 1, displacementsForShape);
    }
}
