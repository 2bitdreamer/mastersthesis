using System.IO;
using UnityEngine;

public class TextureCreator : MonoBehaviour {
    public float m_frequency = 1f;

    [Range(2, 512)]
	public int m_resolution = 512;	

	[Range(1, 8)]
	public int m_octaves = 4;

	[Range(1f, 4f)]
	public float m_lacunarity = 2f;

	[Range(0f, 1f)]
	public float m_persistence = 0.6f;

	[Range(1, 3)]
	public int m_dimensions = 2;

	public NoiseMethodType m_type;

	public Gradient m_coloring;

	public Texture2D m_texture;
	
	public void GenerateLevelTexture () {

		if (m_texture == null) {
			m_texture = new Texture2D(m_resolution, m_resolution, TextureFormat.RGB24, true);
			m_texture.name = "Procedural Texture";
			m_texture.wrapMode = TextureWrapMode.Clamp;
			m_texture.filterMode = FilterMode.Trilinear;
			m_texture.anisoLevel = 9;
			GetComponent<MeshRenderer>().material.mainTexture = m_texture;
		}

		CreateAndSaveTexture();
	}
	
	private void CreateAndSaveTexture() {

        Noise.Shuffle<int>(Noise.hashBase);

        int[] z = new int[Noise.hashBase.Length + Noise.hashBase.Length];
        Noise.hashBase.CopyTo(z, 0);
        Noise.hashBase.CopyTo(z, Noise.hashBase.Length);
        Noise.hash = z;

        if (m_texture.width != m_resolution) {
			m_texture.Resize(m_resolution, m_resolution);
		}
		
		Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f,-0.5f));
		Vector3 point10 = transform.TransformPoint(new Vector3( 0.5f,-0.5f));
		Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
		Vector3 point11 = transform.TransformPoint(new Vector3( 0.5f, 0.5f));

		NoiseMethod method = Noise.methods[(int)m_type][m_dimensions - 1];
		float stepSize = 1f / m_resolution;

		for (int y = 0; y < m_resolution; y++) {
			Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
			for (int x = 0; x < m_resolution; x++) {
				Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
				float sample = Noise.Sum(method, point, m_frequency, m_octaves, m_lacunarity, m_persistence);
				if (m_type != NoiseMethodType.Value) {
					sample = sample * 0.5f + 0.5f;
				}

				m_texture.SetPixel(x, y, m_coloring.Evaluate(sample));
			}
		}

        byte[] bytes = m_texture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/LevelNoise.png", bytes);
    }
}