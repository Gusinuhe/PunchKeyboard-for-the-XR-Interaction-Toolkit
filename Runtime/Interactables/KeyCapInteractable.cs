using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyCapInteractable : AbstractInteractable
{
    [SerializeField] 
    private AudioClip keySound;

    [SerializeField]
    private Key key;

    protected override void Start()
    {
        base.Start();
        
        if (key == null)
            key = GetComponentInChildren<Key>();
    }

    protected override void OnHoverEntered(XRBaseInteractor interactor)
    {
        if (interactor is XRDirectInteractor)
        {
            PlayKeySound();
            key.PressKey();
        }
    }

    protected override void OnHoverExiting(XRBaseInteractor interactor)
    {
        key.ReleaseKey();
    }

    protected override void OnSelectEntering(XRBaseInteractor interactor)
    {
        PlayKeySound();
        key.PressKey();
    }

    protected override void OnSelectExiting(XRBaseInteractor interactor)
    {
        key.ReleaseKey();
        Rigidbody.isKinematic = true;
    }

    private void PlayKeySound()
    {
        AudioSource.PlayClipAtPoint(keySound, transform.position);
    }
}
