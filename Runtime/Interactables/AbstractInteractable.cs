using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class AbstractInteractable : XRGrabInteractable
{
    [SerializeField] 
    protected Rigidbody Rigidbody;

    protected virtual void Start()
    {
        if (Rigidbody == null)
            Rigidbody = GetComponent<Rigidbody>();
    }
    
    /// <inheritdoc />
    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        return interactor is XRRayInteractor;
    }
    
    /// <inheritdoc />
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);

        XRBaseInteractor interactor = args.interactor;
        OnHoverEntered(interactor);
    }

    /// <inheritdoc />
    protected override void OnHoverExiting(HoverExitEventArgs args)
    {
        base.OnHoverExiting(args);
        
        XRBaseInteractor interactor = args.interactor;
        OnHoverExiting(interactor);
    }

    /// <inheritdoc />
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        
        XRBaseInteractor interactor = args.interactor;
        OnSelectEntering(interactor);
    }
    
    /// <inheritdoc />
    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        
        base.OnSelectExiting(args);
        
        XRBaseInteractor interactor = args.interactor;
        OnSelectExiting(interactor);
    }
    
    protected abstract void OnHoverEntered(XRBaseInteractor interactor);
    protected abstract void OnHoverExiting(XRBaseInteractor interactor);
    protected abstract void OnSelectEntering(XRBaseInteractor interactor);
    protected abstract void OnSelectExiting(XRBaseInteractor interactor);
}
