using UnityEngine;

namespace Player.New
{
    public static class HitboxUtils
    {
        /// <summary>
        /// Devuelve el IDamageable más cercano en un cono frontal.
        /// </summary>
        /// <param name="origin">Punto de origen del ataque</param>
        /// <param name="forward">Dirección frontal del atacante (plano XZ)</param>
        /// <param name="range">Alcance del ataque</param>
        /// <param name="halfAngleDeg">Semiancho del cono en grados (e.g., 55)</param>
        /// <param name="mask">LayerMask de enemigos</param>
        /// <param name="target">Objetivo encontrado (o null)</param>
        /// <returns>true si encontró objetivo</returns>
        public static bool TryGetClosestInFront(Vector3 origin, Vector3 forward, float range, float halfAngleDeg,
            LayerMask mask, out IDamageable target, out Transform targetTransform)
        {
            target = null;
            targetTransform = null;

            Collider[] hits = Physics.OverlapSphere(origin, range, mask, QueryTriggerInteraction.Ignore);
            if (hits == null || hits.Length == 0) return false;

            float bestSqr = float.PositiveInfinity;
            Vector3 fwdXZ = forward; fwdXZ.y = 0f; fwdXZ = fwdXZ.sqrMagnitude > 1e-6f ? fwdXZ.normalized : Vector3.forward;

            foreach (var c in hits)
            {
                // Buscar en el collider o sus padres
                IDamageable d = c.GetComponentInParent<IDamageable>();
                if (d == null) continue;

                Vector3 to = c.bounds.center - origin;
                Vector3 toXZ = new Vector3(to.x, 0f, to.z);
                float sqr = toXZ.sqrMagnitude;
                if (sqr > range * range) continue;

                // Angulo frontal
                float ang = Vector3.Angle(fwdXZ, toXZ);
                if (ang > halfAngleDeg) continue;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    target = d;
                    targetTransform = c.transform;
                }
            }
            return target != null;
        }
    }
}
