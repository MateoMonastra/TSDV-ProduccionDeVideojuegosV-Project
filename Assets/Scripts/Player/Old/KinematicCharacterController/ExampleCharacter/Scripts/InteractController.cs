using UnityEngine;

namespace KinematicCharacterController.Examples
{
    public class InteractController : MonoBehaviour
    {
        private IInteractable interactionTarget;

        public void Interact()
        {
            if (interactionTarget != null)
            {
                interactionTarget.Interact();
            }
        }

        public void DetectInteractions()
        {
            Collider[] colls = Physics.OverlapSphere(transform.position, 22.0f, LayerMask.GetMask("Interact"));

            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            foreach (var VARIABLE in colls)
            {
                if (VARIABLE.TryGetComponent(out IInteractable interactable))
                {
                    if (closestInteractable == null)
                    {
                        closestInteractable = interactable;
                    }
                    

                    float newDistance = Vector3.Distance(interactable.GetInteractionPoint(), transform.position);

                    
                    
                    if (interactable.TryInteractionRange(transform.position) && newDistance < closestDistance)
                    {
                        closestInteractable = interactable;
                        closestDistance = newDistance;
                    }
                    else
                    {
                        interactable.ToggleIndicator(false);
                    }
                }
            }

            if (closestInteractable != null && Vector3.Distance(closestInteractable.GetInteractionPoint(),transform.position) < 20.0f)
            {
                closestInteractable.ToggleIndicator(true);
                interactionTarget = closestInteractable;
            }
            else if(closestInteractable != null)
            {
                closestInteractable.ToggleIndicator(false);
            }
        }
    }
}