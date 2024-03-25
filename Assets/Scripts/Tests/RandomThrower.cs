using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomThrower : MonoBehaviour
{
    private GameObject _target;
    void Start()
    {
        _target = (GameObject)Resources.Load("Prefab/P_TestObj");
        StartCoroutine(Throw());
    }

    IEnumerator Throw() 
    {
        yield return new WaitForSeconds(1f);
        while (true) 
        {
           GameObject obj =   Instantiate(_target);
            obj.transform.position = transform.position;
            obj.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)) * 5f;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
