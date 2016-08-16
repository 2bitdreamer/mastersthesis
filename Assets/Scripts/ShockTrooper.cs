using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShockTrooper : Unit {

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
        Initialize(pos, 5, '#', team, 1f, 5, 1, 2, 4, 2, 2, 1, displacementsForShape);
    }
}
