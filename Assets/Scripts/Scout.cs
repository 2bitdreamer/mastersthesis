using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scout : Unit {

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
        displacementsForShape.Add(new TileCoord(-1, 0));
        displacementsForShape.Add(new TileCoord(-1, 1));
        displacementsForShape.Add(new TileCoord(0, 1));
        displacementsForShape.Add(new TileCoord(1, 0));
        displacementsForShape.Add(new TileCoord(1, -1));
        displacementsForShape.Add(new TileCoord(0, -1));

        Initialize(pos, 7, '!', team, 1f, 4, 2, 3, 1, 1, 1, 2, displacementsForShape);
    }
}
