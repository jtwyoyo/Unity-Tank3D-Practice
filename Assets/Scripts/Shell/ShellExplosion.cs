using UnityEngine;
using MyCompany.Tank3D.Tank;

namespace MyCompany.Tank3D.Shell
{
    public class ShellExplosion : MonoBehaviour
    {
        [SerializeField] private LayerMask m_TankMask;
        [SerializeField] private ParticleSystem m_ExplosionParticles;
        [SerializeField] private AudioSource m_ExplosionAudio;
        
        private const float m_MaxDamage = 100f;
        private const float m_ExplosionForce = 1000f;
        private const float m_MaxLifeTime = 2f;
        private const float m_ExplosionRadius = 5f;

        private void Start()
        {
            Destroy(gameObject, m_MaxLifeTime);
        }


        private void OnTriggerEnter(Collider other)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

            for (int i = 0; i < colliders.Length; i++)
            {
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

                if (!targetRigidbody) continue;

                targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

                if (!targetHealth) continue;

                float damage = CalculateDamage(targetRigidbody.position);

                targetHealth.TakeDamage(damage);
            }

            m_ExplosionParticles.transform.parent = null;

            m_ExplosionParticles.Play();

            Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.duration);
            Destroy(gameObject);
        }


        private float CalculateDamage(Vector3 targetPosition)
        {
            Vector3 explosionToTarget = targetPosition - transform.position;

            float explosionDistance = explosionToTarget.magnitude;

            float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

            float damage = relativeDistance * m_MaxDamage;

            damage = Mathf.Max(0f, damage);

            return damage;
        }
    }
}