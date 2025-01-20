using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class PlayerController : MonoBehaviour
{

    private NavMeshAgent _playerAgent; 


    void Awake()
    {
        _playerAgent = GetComponent<NavMeshAgent>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire2"))
        {
            SetDestinationPoint(); 
        }
    }


    void SetDestinationPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
        RaycastHit hit; 
        if(Physics.Raycast(ray, out hit))
        {
            if(hit.transform.gameObject.layer == 6 )
            {
                _playerAgent.destination = hit.point; 
                //_playerAgent.SetDestinationPoint(hit.point); 
            }
        }
    }
}
