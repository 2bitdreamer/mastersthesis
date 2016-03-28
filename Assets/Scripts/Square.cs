using UnityEngine;
using System.Collections;

public class Square : MonoBehaviour {
	
	public float m_Length = 1.0f;
	public float m_Width = 1.0f;
	public Color m_Color = Color.green;
	
	// Use this for initialization
	void Start () {
		GetComponent<Renderer>().material.color = m_Color;

		//makeSquare ();
		GameObject hex1 = (GameObject)(Instantiate (Resources.Load ("hexTest"),new Vector3(0.0f,0.0f,0.0f),Quaternion.identity));
		GameObject hex2 = (GameObject)(Instantiate (Resources.Load ("hexTest"),new Vector3(-2.0f,-2.0f,0.0f),Quaternion.identity));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void makeSquare()
	{
		Vector3[] vertices = new Vector3[4];
		
		vertices[0] = new Vector2(0.0f, 0.0f);
		vertices[1] = new Vector2(0.0f, m_Length);
		vertices[2] = new Vector2(m_Width, m_Length);
		vertices[3] = new Vector2(m_Width, 0.0f);
		
		
		Color[] colors = new Color[4];
		for (int i = 0; i < 4; i++)
		{
			colors[i] = m_Color;
		}
		
		
		int[] indices = new int[6]; //2 triangles, 3 indices each
		
		indices[0] = 0;
		indices[1] = 1;
		indices[2] = 2;
		
		indices[3] = 0;
		indices[4] = 2;
		indices[5] = 3;
		
		Mesh mesh = new Mesh();
		
		mesh.vertices = vertices;
		
		mesh.triangles = indices;
		
		mesh.RecalculateBounds();
		
		MeshFilter filter = GetComponent<MeshFilter>();
		
		if (filter != null)
		{
			filter.sharedMesh = mesh;
		}
	}
}
