using System.Collections;
using UnityEngine;

public class doorscript : MonoBehaviour
{
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool isOpen = false;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;

    void Start() // <-- must be capitalized so Unity calls it
    {
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(ToggleDoor());
        }
    }

    private IEnumerator ToggleDoor()
    {
        Quaternion targetRotation = isOpen ? _closedRotation : _openRotation;
        Quaternion startRotation = transform.rotation;
        isOpen = !isOpen;

        float timeElapsed = 0f;
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            timeElapsed += Time.deltaTime * openSpeed;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, timeElapsed);
            yield return null;
        }

        transform.rotation = targetRotation; // snap to final position
    }
}
