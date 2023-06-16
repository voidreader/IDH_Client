using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace EpicToonFX
{
public class ETFXAutoFireProjectile : MonoBehaviour 
{
    RaycastHit hit;
    public GameObject[] projectiles;
    public Transform spawnPosition;
    [HideInInspector]
    public int currentProjectile = 0;
	//public float speed = 1000;
        public float m_startDelay;
        public float fHowManyTimes = 1;

        float m_Time;
        Vector3 startPos;

        public Vector2 fRndSpeed = new Vector2();

        void Start () 
	{
        m_Time = Time.time;
            GameObject teamPos = GameObject.Find("LeftTeamPos(Clone)");
            startPos = new Vector3(teamPos.transform.position.x - 10, teamPos.transform.position.y + 6.5f, teamPos.transform.position.z);

             GameObject projectile = Instantiate(projectiles[currentProjectile], startPos, Quaternion.identity) as GameObject;
            //GameObject projectile = Instantiate(projectiles[currentProjectile], spawnPosition.position, Quaternion.identity) as GameObject;
            //projectile.transform.LookAt(hit.point);
            //projectile.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
            //projectile.transform.LookAt(Vector3.right);
            //projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * speed);
            float speed = UnityEngine.Random.Range(fRndSpeed.x, fRndSpeed.y);
            projectile.GetComponent<Rigidbody>().AddForce( (projectile.transform.right) * speed);
            projectile.GetComponent<ETFXProjectileScript>().impactNormal = hit.normal;
    }

	void Update () 
	{

            if (Time.time > m_Time + m_startDelay && fHowManyTimes > 0)
            {
                Vector3 vRndZ = new Vector3(0, 0, UnityEngine.Random.Range(-4, 5));
                GameObject projectile = Instantiate(projectiles[currentProjectile], startPos + vRndZ, Quaternion.identity) as GameObject;
                //GameObject projectile = Instantiate(projectiles[currentProjectile], spawnPosition.position, Quaternion.identity) as GameObject;
                //projectile.transform.LookAt(hit.point);
                //projectile.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                //projectile.transform.LookAt(Vector3.right);
                //projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * speed);
                float speed = UnityEngine.Random.Range(fRndSpeed.x, fRndSpeed.y);
                projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.right * speed);
                projectile.GetComponent<ETFXProjectileScript>().impactNormal = hit.normal;

                Transform[] tms = projectile.GetComponentsInChildren<Transform>(true);
                foreach (Transform tm in tms)
                {
                    tm.gameObject.layer = this.transform.gameObject.layer;
                }

                m_Time = Time.time;

                fHowManyTimes--;
            }
        }


}
}