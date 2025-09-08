using System;
using System.Collections;
using System.Data;
using Player.New;
using UnityEngine;

namespace KinematicCharacterController.Examples
{
    public class InteractController : MonoBehaviour
    {
        private IInteractable interactionTarget;
        private Coroutine _interactionCoroutine;

        public Action<InteractData> OnStartInteractAction;
        public Action<InteractData> OnEndInteractAction;


        public void Interact()
        {
            if (interactionTarget != null)
            {
                InteractData data = interactionTarget.Interact(false);

                if (data.successInteraction)
                {
                    _interactionCoroutine = StartCoroutine(InteractionCoroutine(data));
                }
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

        private IEnumerator InteractionCoroutine(InteractData data)
        {
            OnStartInteractAction?.Invoke(data);
            yield return new WaitForSeconds(data.interactionTime);
            OnEndInteractAction?.Invoke(data);
            interactionTarget.FinishInteraction();
        }

        public void InterruptInteraction()
        {
            StopCoroutine(_interactionCoroutine);
            interactionTarget.InterruptInteraction();
        }
    }
}