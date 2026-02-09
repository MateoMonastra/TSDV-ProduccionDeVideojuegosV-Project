using System;
using System.Collections;
using UnityEngine;

namespace Player.Old.KinematicCharacterController.ExampleCharacter.Scripts
{
    public class InteractController : MonoBehaviour
    {
        private IInteractable _interactionTarget;
        private Coroutine _interactionCoroutine;

        public Action<InteractData> OnStartInteractAction;
        public Action<InteractData> OnEndInteractAction;
        
       [SerializeField] private float detectionRadius = 22f;


        public void Interact()
        {
            if (_interactionTarget != null)
            {
                InteractData data = _interactionTarget.Interact(false);

                if (data.successInteraction)
                {
                    _interactionCoroutine = StartCoroutine(InteractionCoroutine(data));
                }
            }
        }

        public void DetectInteractions()
        {
            Collider[] colls = Physics.OverlapSphere(transform.position, detectionRadius, LayerMask.GetMask("Interact"));

            _interactionTarget = null;
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
                _interactionTarget = closestInteractable;
            }
        }

        private IEnumerator InteractionCoroutine(InteractData data)
        {
            OnStartInteractAction?.Invoke(data);
            yield return new WaitForSeconds(data.interactionTime);
            OnEndInteractAction?.Invoke(data);
            _interactionTarget?.FinishInteraction();
        }

        public void InterruptInteraction()
        {
            if (_interactionCoroutine != null)
                StopCoroutine(_interactionCoroutine);
            
            _interactionTarget?.InterruptInteraction();
        }
    }
}