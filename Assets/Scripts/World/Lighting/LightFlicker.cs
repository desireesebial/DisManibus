using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds flicker behavior to a Light and (optionally) syncs emissive material intensity on a fixture.
/// Attach to a GameObject with a Light or assign a target light explicitly.
/// </summary>
[DisallowMultipleComponent]
public class LightFlicker : MonoBehaviour
{
	public enum FlickerMode
	{
		SmoothNoise,
		RandomBursts,
		Blink
	}

	[Header("Target")]
	[SerializeField] private Light targetLight;
	[Tooltip("If enabled, scales emissive color on the renderer to match the light intensity.")]
	[SerializeField] private bool controlEmission = false;
	[SerializeField] private Renderer emissionRenderer;
	[Tooltip("Emission color property name on the material (usually _EmissionColor).")]
	[SerializeField] private string emissionColorProperty = "_EmissionColor";
	[Tooltip("Multiplier applied to emission relative to base intensity. 1 = match light.")]
	[SerializeField] private float emissionIntensityMultiplier = 1.0f;

	[Header("Common Intensity Settings")]
	[Min(0f)]
	[SerializeField] private float baseIntensity = 1.0f;
	[Tooltip("When mode uses variance, intensity fluctuates within ±variance around base.")]
	[Min(0f)]
	[SerializeField] private float intensityVariance = 0.5f;

	[Header("Mode")] 
	[SerializeField] private FlickerMode mode = FlickerMode.SmoothNoise;

	[Header("Smooth Noise Settings")] 
	[Tooltip("How fast the noise evolves over time.")]
	[Min(0f)]
	[SerializeField] private float noiseSpeed = 1.5f;

	[Header("Random Bursts Settings")] 
	[Tooltip("Avg number of burst events per second.")]
	[Min(0f)]
	[SerializeField] private float burstProbabilityPerSecond = 0.25f;
	[Tooltip("During a burst, intensity is reduced to this factor of base.")]
	[Range(0f, 1f)]
	[SerializeField] private float burstDimFactor = 0.1f;
	[Tooltip("Duration range (seconds) for a single burst event.")]
	[Min(0f)]
	[SerializeField] private Vector2 burstDurationRange = new Vector2(0.05f, 0.25f);

	[Header("Blink Settings")]
	[Tooltip("Randomized interval range between blinks (seconds).")]
	[Min(0f)]
	[SerializeField] private Vector2 blinkIntervalRange = new Vector2(1f, 5f);
	[Tooltip("How long the light stays OFF during a blink (seconds).")]
	[Min(0f)]
	[SerializeField] private float blinkOffDuration = 0.08f;

	// Internal state
	private float noiseOffset;
	private bool isInBurst;
	private float nextBlinkAt;
	private float originalBaseIntensity;

	// Emission caching
	private bool emissionSupported;
	private List<Color> originalEmissionColors;
	private List<Material> materialsInstance;

	private void Reset()
	{
		TryAutoAssigns();
	}

	private void Awake()
	{
		TryAutoAssigns();
	}

	private void OnEnable()
	{
		if (targetLight != null)
		{
			originalBaseIntensity = Mathf.Max(0f, baseIntensity <= 0f ? targetLight.intensity : baseIntensity);
		}
		noiseOffset = Random.value * 1000f;
		PrepareEmission();
		ScheduleNextBlink();
	}

	private void Update()
	{
		if (targetLight == null)
		{
			return;
		}

		switch (mode)
		{
			case FlickerMode.SmoothNoise:
				UpdateSmoothNoise();
				break;
			case FlickerMode.RandomBursts:
				UpdateRandomBursts();
				break;
			case FlickerMode.Blink:
				UpdateBlink();
				break;
		}
	}

	private void UpdateSmoothNoise()
	{
		float noise = Mathf.PerlinNoise(noiseOffset, Time.time * Mathf.Max(0.01f, noiseSpeed));
		float centered = (noise - 0.5f) * 2f; // [-1,1]
		float targetIntensity = Mathf.Max(0f, baseIntensity + centered * intensityVariance);
		ApplyIntensity(targetIntensity);
	}

	private void UpdateRandomBursts()
	{
		if (!isInBurst)
		{
			float p = burstProbabilityPerSecond * Time.deltaTime;
			if (Random.value < p)
			{
				StartCoroutine(BurstRoutine());
			}
			else
			{
				// Idle subtle jitter so it does not look static
				float noise = Mathf.PerlinNoise(noiseOffset, Time.time * 0.8f);
				float centered = (noise - 0.5f) * 2f;
				float targetIntensity = Mathf.Max(0f, baseIntensity + centered * (intensityVariance * 0.5f));
				ApplyIntensity(targetIntensity);
			}
		}
	}

	private IEnumerator BurstRoutine()
	{
		isInBurst = true;
		float duration = Random.Range(Mathf.Min(burstDurationRange.x, burstDurationRange.y), Mathf.Max(burstDurationRange.x, burstDurationRange.y));
		float endTime = Time.time + duration;
		float dimIntensity = Mathf.Max(0f, baseIntensity * burstDimFactor);
		while (Time.time < endTime)
		{
			// Add quick jitter during burst
			float jitter = Random.Range(-intensityVariance, intensityVariance) * 0.25f;
			ApplyIntensity(Mathf.Max(0f, dimIntensity + jitter));
			yield return null;
		}
		isInBurst = false;
	}

	private void UpdateBlink()
	{
		if (Time.time >= nextBlinkAt)
		{
			StartCoroutine(BlinkRoutine());
			ScheduleNextBlink();
		}
		else
		{
			// Gentle tiny movement to avoid looking perfectly static
			float noise = Mathf.PerlinNoise(noiseOffset, Time.time * 0.5f);
			float centered = (noise - 0.5f) * 2f;
			float targetIntensity = Mathf.Max(0f, baseIntensity + centered * (intensityVariance * 0.25f));
			ApplyIntensity(targetIntensity);
		}
	}

	private IEnumerator BlinkRoutine()
	{
		// Brief spike down to 0 then restore
		bool wasEnabled = targetLight.enabled;
		float previousIntensity = targetLight.intensity;
		targetLight.enabled = true; // ensure visible flicker even if previously off
		ApplyIntensity(0f);
		yield return new WaitForSeconds(Mathf.Max(0f, blinkOffDuration));
		ApplyIntensity(baseIntensity);
		targetLight.enabled = wasEnabled;
	}

	private void ScheduleNextBlink()
	{
		float min = Mathf.Max(0f, Mathf.Min(blinkIntervalRange.x, blinkIntervalRange.y));
		float max = Mathf.Max(blinkIntervalRange.x, blinkIntervalRange.y);
		nextBlinkAt = Time.time + Random.Range(min, Mathf.Max(min + 0.0001f, max));
	}

	private void ApplyIntensity(float targetIntensity)
	{
		targetLight.intensity = targetIntensity;
		if (controlEmission && emissionSupported)
		{
			if (materialsInstance == null || originalEmissionColors == null)
			{
				return;
			}
			float safeBase = Mathf.Max(0.0001f, originalBaseIntensity);
			float scale = (targetIntensity / safeBase) * Mathf.Max(0f, emissionIntensityMultiplier);
			for (int i = 0; i < materialsInstance.Count; i++)
			{
				Material mat = materialsInstance[i];
				if (mat == null)
				{
					continue;
				}
				Color baseColor = originalEmissionColors[i];
				Color adjusted = baseColor * scale;
				mat.SetColor(emissionColorProperty, adjusted);
			}
		}
	}

	private void PrepareEmission()
	{
		emissionSupported = false;
		originalEmissionColors = null;
		materialsInstance = null;

		if (!controlEmission)
		{
			return;
		}
		if (emissionRenderer == null)
		{
			emissionRenderer = GetComponentInChildren<Renderer>();
		}
		if (emissionRenderer == null)
		{
			return;
		}

		// Use instance materials so we do not affect shared assets
		materialsInstance = new List<Material>();
		originalEmissionColors = new List<Color>();
		Material[] mats = emissionRenderer.materials;
		for (int i = 0; i < mats.Length; i++)
		{
			Material instanceMat = mats[i];
			materialsInstance.Add(instanceMat);
			Color baseColor = instanceMat.HasProperty(emissionColorProperty) ? instanceMat.GetColor(emissionColorProperty) : Color.black;
			originalEmissionColors.Add(baseColor);
		}
		emissionSupported = materialsInstance.Count > 0;
	}

	private void TryAutoAssigns()
	{
		if (targetLight == null)
		{
			targetLight = GetComponentInChildren<Light>();
		}
	}
}


