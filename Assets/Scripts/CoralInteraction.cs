using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CoralInteraction : MonoBehaviour
{
    [SerializeField] private GameObject _raycastOrigin;
    [SerializeField] private float _raycastDistance;
    [SerializeField] private GameObject _examinePrompt;
    [SerializeField] private FirstPersonController _firstPersonController;
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private GameObject _lightObjects;
    [SerializeField] private GameObject _darkObjects;
    [SerializeField] private CoralColorTransitionManager _colorTransitionManager;
    [SerializeField] private Image _blackout;
    [SerializeField] private float _lookRoutineDuration;
    [SerializeField] private float _blackingOutDuration;
    [SerializeField] private float _blackoutDuration;

    private bool _isLookingAtCoral;
    private bool _hasBeenInspected;
    private GameObject _target;
    private Camera _playerCamera;

    private void Awake()
    {
        _playerCamera = _firstPersonController.playerCamera;
    }

    private void Update()
    {
        if (_isLookingAtCoral && Input.GetKeyDown(KeyCode.E))
        {
            StartInspectCoral();
        }
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(_raycastOrigin.transform.position, _raycastOrigin.transform.forward, out RaycastHit hit, _raycastDistance))
        {
            if (hit.collider.CompareTag("Coral"))
            {
                if (!_hasBeenInspected)
                {
                    _target = hit.transform.gameObject;
                    SetLookingAtCoral(true);
                    return;
                }
            }
        }

        SetLookingAtCoral(false);
    }

    private void SetLookingAtCoral(bool lookingAtCoral)
    {
        _isLookingAtCoral = lookingAtCoral;
        _examinePrompt.SetActive(lookingAtCoral);
    }

    private void StartInspectCoral()
    {
        _hasBeenInspected = true;
        _firstPersonController.enabled = false;
        _rigidBody.velocity = Vector3.zero;
        
        StartCoroutine(InspectCoralRoutine());
    }

    private IEnumerator InspectCoralRoutine()
    {
        float elapsedTime = 0;

        Quaternion startRotation = _firstPersonController.playerCamera.transform.rotation;
        float startFOV = _playerCamera.fieldOfView;

        Vector3 lookForward = _target.transform.position + Vector3.down * .5f - _firstPersonController.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(lookForward);

        while (elapsedTime <= _lookRoutineDuration)
        {
            float t = EaseOutCubic(elapsedTime / _lookRoutineDuration);
            _playerCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            _playerCamera.fieldOfView = Mathf.Lerp(startFOV, 15f, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;

        while (elapsedTime <= _blackingOutDuration)
        {
            _blackout.color = new Color(0f, 0f, 0f, elapsedTime / _blackingOutDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _lightObjects.SetActive(false);
        _darkObjects.SetActive(true);
        yield return new WaitForSeconds(_blackoutDuration);

        _blackout.color = new Color(0, 0, 0, 0);

        elapsedTime = 0;

        while (elapsedTime <= _lookRoutineDuration * 0.15f)
        {
            float t = EaseInCubic(elapsedTime / (_lookRoutineDuration * 0.15f));
            _playerCamera.transform.rotation = Quaternion.Lerp(targetRotation, startRotation, t);
            _playerCamera.fieldOfView = Mathf.Lerp(15f, startFOV, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _firstPersonController.enabled = true;
    }
    
    private float EaseOutCubic(float x)
    {
        return 1 - Mathf.Pow(1 - x, 3);
    }

    private float EaseInCubic(float x)
    {
        return Mathf.Pow(x, 3);
    }
}
