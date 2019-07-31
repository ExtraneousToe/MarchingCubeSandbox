using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProceduralNoiseProject;

[CreateAssetMenu(fileName = "Noise Layer")]
public class NoiseLayerScriptable : ScriptableObject
{
	[SerializeField]
	private bool isEnabled = false;
	public bool IsEnabled => isEnabled;

	[SerializeField]
	private NOISE_TYPE m_noiseType = NOISE_TYPE.PERLIN;
	public NOISE_TYPE NoiseType => m_noiseType;

	[SerializeField]
	private int m_seed = 0;
	public int Seed => m_seed;

	[SerializeField]
	[Range(1,16)]
	private int m_octaves = 1;
	public int Octaves => Mathf.Max(1, m_octaves);

	[SerializeField]
	private float m_frequency = 1f;
	public float Frequency => m_frequency;

	[SerializeField]
	private float m_noiseAmplitude = 1f;
	public float NoiseAmplitude => m_noiseAmplitude;

	[SerializeField]
	private float m_fractalAmplitude = 1f;
	public float FractalAmplitude => m_fractalAmplitude;

	[SerializeField]
	private int m_raiseHeight = 0;
	public int RaiseHeight => m_raiseHeight;

	private FractalNoise fracNoise;

	public void Initialise()
	{
		fracNoise = new FractalNoise(GetNoise(NoiseType, Seed, Frequency, NoiseAmplitude), Octaves, Frequency, FractalAmplitude);
	}

	public float SampleValue(float x, float z)
	{
		if (!IsEnabled) return 0;

		//return RaiseHeight + (fracNoise.Sample2D(x, z) + fracNoise.Amplitude * fracNoise.Noises.Max(n => n.Amplitude));
		return RaiseHeight + fracNoise.Sample2D(x, z);
	}

	private static INoise GetNoise(NOISE_TYPE noiseType = NOISE_TYPE.PERLIN, int seed = 0, float frequency = 20f, float amplitude = 1f)
	{
		switch (noiseType)
		{
			case NOISE_TYPE.PERLIN:
				return new PerlinNoise(seed, frequency, amplitude);

			case NOISE_TYPE.VALUE:
				return new ValueNoise(seed, frequency, amplitude);

			case NOISE_TYPE.SIMPLEX:
				return new SimplexNoise(seed, frequency, amplitude);

			case NOISE_TYPE.VORONOI:
				return new VoronoiNoise(seed, frequency, amplitude);

			case NOISE_TYPE.WORLEY:
				return new WorleyNoise(seed, frequency, amplitude);

			default:
				return new PerlinNoise(seed, frequency, amplitude);
		}
	}
}
