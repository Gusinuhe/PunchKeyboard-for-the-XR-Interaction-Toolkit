using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyboardInteractable : XRGrabInteractable
{
    private Vector3 initialAttachPosition;
    private Quaternion initialAttachRotation;
    
    /// <inheritdoc />
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);

        XRBaseInteractor interactor = args.interactor;
        
        initialAttachPosition = interactor.attachTransform.localPosition;
        initialAttachRotation = interactor.attachTransform.localRotation;
        
        interactor.attachTransform.SetPositionAndRotation(transform.position, transform.rotation);
    }

    /// <inheritdoc />
    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        XRBaseInteractor interactor = args.interactor;
        
        interactor.attachTransform.localPosition = initialAttachPosition;
        interactor.attachTransform.localRotation = initialAttachRotation;
    }
}
