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

            interactionTarget = null;
            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            foreach (var VARIABLE in colls)
            {
                if (VARIABLE.TryGetComponent(out IInteractable interactable))
                {
                    float newDistance = Vector3.Distance(interactable.GetInteractionPoint(), transform.position);

                    if (interactable.TryInteractionRange(transform.position) && newDistance < closestDistance)
                    {
                        closestInteractable = interactable;
                        closestDistance = newDistance;
                    }
                    else
                    {
                        interactable.SetIndicator(false);
                    }
                }
            }

            if (closestInteractable != null)
            {
                closestInteractable.SetIndicator(true);
                interactionTarget = closestInteractable;
            }
        }
    }
}