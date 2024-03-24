using UnityEngine;

public class SimpleShooter : MonoBehaviour
{
    private GameObject _object;

    private GameObject[] _object_pool;

    public int PoolSize = 20;
    private int _index = 0;

    public float TimeBetweenShoot = 0.5f;
    public float BulletVelocity = 10f;

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        _object = (GameObject)Resources.Load("Prefab/P_TestObj");
        _object_pool = new GameObject[PoolSize];
        for (int i = 0; i < _object_pool.Length; i++) 
        {
            _object_pool[i] = Instantiate(_object);
            _object_pool[i].SetActive(false);
        }
    }
    private void Update()
    {
        if (Input.GetMouseButton(0))
            Shoot();
    }
    private void Shoot() 
    {
        GameObject currentObj = _object_pool[_index];
        currentObj.SetActive(true);
        currentObj.transform.position = transform.position + transform.forward * 2f;
        currentObj.GetComponent<Rigidbody>().velocity = transform.forward * BulletVelocity;
        _index++;
        _index %= _object_pool.Length;
    }
}
