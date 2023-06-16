using UnityEngine;
using System.Collections;
 
public class ETFXProjectileScript : MonoBehaviour
{
    public GameObject impactParticle;
    public GameObject projectileParticle;
    public GameObject muzzleParticle;
    public GameObject[] trailParticles;
    [HideInInspector]
    public Vector3 impactNormal; //Used to rotate impactparticle.
 
    private bool hasCollided = false;
 
    void Start()
    {
        projectileParticle = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
        Transform[] tms = projectileParticle.GetComponentsInChildren<Transform>(true);
        foreach (Transform tm in tms)
        {
            tm.gameObject.layer = this.transform.gameObject.layer;
        }
        projectileParticle.transform.parent = transform;

		if (muzzleParticle)
        {
            muzzleParticle = Instantiate(muzzleParticle, transform.position, transform.rotation) as GameObject;
            tms = muzzleParticle.GetComponentsInChildren<Transform>(true);
            foreach (Transform tm in tms)
            {
                tm.gameObject.layer = this.transform.gameObject.layer;
            }

            Destroy(muzzleParticle, 1.5f); // Lifetime of muzzle effect.
		}
    }

    public void Update()
    {
        if(transform.position.y < -1.0f)
        {
            Explosion();
        }
    }

    void OnCollisionEnter(Collision hit)
    {
        if (!hasCollided)
        {
            Explosion();
        }
    }

    public void Explosion()
    {
        hasCollided = true;
        //transform.DetachChildren();
        impactParticle = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
        Transform[] tms = impactParticle.GetComponentsInChildren<Transform>(true);
        foreach (Transform tm in tms)
        {
            tm.gameObject.layer = this.transform.gameObject.layer;
        }
        //Debug.DrawRay(hit.contacts[0].point, hit.contacts[0].normal * 1, Color.yellow);

        /*
        if (hit.gameObject.tag == "Destructible") // Projectile will destroy objects tagged as Destructible
        {
            Destroy(hit.gameObject);
        }
        */

        //yield WaitForSeconds (0.05);
        foreach (GameObject trail in trailParticles)
        {
            GameObject curTrail = transform.Find(projectileParticle.name + "/" + trail.name).gameObject;
            curTrail.transform.parent = null;
            Destroy(curTrail, 3f);
        }
        Destroy(projectileParticle, 3f);
        Destroy(impactParticle, 5f);
        Destroy(gameObject);
        //projectileParticle.Stop();

        ParticleSystem[] trails = GetComponentsInChildren<ParticleSystem>();
        //Component at [0] is that of the parent i.e. this object (if there is any)
        for (int i = 1; i < trails.Length; i++)
        {
            ParticleSystem trail = trails[i];
            if (!trail.gameObject.name.Contains("Trail"))
                continue;

            trail.transform.SetParent(null);
            Destroy(trail.gameObject, 2);
        }
    }
}