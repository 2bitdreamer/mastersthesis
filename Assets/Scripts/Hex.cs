using UnityEngine;
using System.Collections;

public class Hex : MonoBehaviour {
	public float m_Rad = 1.0f;
	public Color m_Color = Color.green;

	// Use this for initialization
	void Start () {
		GetComponent<Renderer>().material.color = m_Color;
		MakeHex(transform.position.x, transform.position.y);
	}
	
	// Update is called once per frame
	void Update () {

	}

	void MakeHex(float xPos, float yPos){
		Vector3[] vertices = new Vector3[7];
		for (int i = 0; i < 6; i++) {
			float angle_deg = (-60 * i) + 30;
			float angle_radians = Mathf.Deg2Rad * angle_deg;
			float vertX = xPos+ (m_Rad * Mathf.Cos(angle_radians));
			float vertY = yPos + (m_Rad * Mathf.Sin(angle_radians));
			vertices[i] = new Vector2(vertX,vertY);
		}

		vertices[6] = new Vector2 (xPos, yPos);

		//int[] indices = new int[18]; //2 triangles, 3 indices each
		int[] indices = new int[18]; //2 triangles, 3 indices each
		
		indices[0] = 6;
		indices[1] = 0;
		indices[2] = 1;
		
		indices[3] = 6;
		indices[4] = 1;
		indices[5] = 2;

		indices[6] = 6;
		indices[7] = 2;
		indices[8] = 3;

		indices[9] = 6;
		indices[10] = 3;
		indices[11] = 4;

		indices[12] = 6;
		indices[13] = 4;
		indices[14] = 5;

		indices[15] = 6;
		indices[16] = 5;
		indices[17] = 0;

		Mesh mesh = new Mesh();
		
		mesh.vertices = vertices;
		
		mesh.triangles = indices;

		mesh.RecalculateBounds();
		
		MeshFilter filter = GetComponent<MeshFilter>();
		filter.sharedMesh = mesh;
	}
}
