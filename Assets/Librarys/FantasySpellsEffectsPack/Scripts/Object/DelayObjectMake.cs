using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayObjectMake : _ObjectMakeBase{

    public float m_startDelay;
    float m_Time;
    bool isMade = false;

    public bool IsRandomPos = false;
    public Vector3 vMinRandomPos = new Vector3(); 
    public Vector3 vMaxRandomPos = new Vector3(); 

    void Start(){
        m_Time = Time.time;
    }

    void Update()
    {
        if (IsRandomPos)
        {        
            for (int i = 0; i < m_makeObjs.Length; i++)
            {
                if (Time.time > m_Time + m_startDelay)
                {
                    GameObject m_obj = Instantiate(m_makeObjs[i], transform.position, transform.rotation);
                    m_obj.transform.parent = this.transform;
                    m_obj.transform.localPosition = new Vector3(
                        UnityEngine.Random.Range(vMinRandomPos.x, vMaxRandomPos.x + 1),
                        0,
                        UnityEngine.Random.Range(vMinRandomPos.z, vMaxRandomPos.z + 1));


                    Transform[] tms = m_obj.GetComponentsInChildren<Transform>(true);
                    foreach (Transform tm in tms)
                    {
                        tm.gameObject.layer = this.transform.gameObject.layer;
                    }

                    m_Time = Time.time;
                }
            }        
        }
        else
        {
            if (Time.time > m_Time + m_startDelay && !isMade)
            {
                isMade = true;
                for (int i = 0; i < m_makeObjs.Length; i++)
                {
                    GameObject m_obj = Instantiate(m_makeObjs[i], transform.position, transform.rotation);
                    m_obj.transform.parent = this.transform;

                    m_obj.transform.localPosition = new Vector3(
                        UnityEngine.Random.Range(vMinRandomPos.x, vMaxRandomPos.x + 1),
                        0,
                        UnityEngine.Random.Range(vMinRandomPos.z, vMaxRandomPos.z + 1));


                    Transform[] tms = m_obj.GetComponentsInChildren<Transform>(true);
                    foreach (Transform tm in tms)
                    {
                        tm.gameObject.layer = this.transform.gameObject.layer;
                    }


                    if (m_movePos)
                    {
                        if (m_obj.GetComponent<MoveToObject>())
                        {
                            MoveToObject m_script = m_obj.GetComponent<MoveToObject>();
                            m_script.m_movePos = m_movePos;
                        }
                    }
                }
            }
        }
    }
}
