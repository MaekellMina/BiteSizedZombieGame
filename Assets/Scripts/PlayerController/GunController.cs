using UnityEngine;

public class GunController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;


    public void Fire()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - firePoint.position).normalized;

        Debug.Log("PEW");
        //GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        //bullet.GetComponent<Bullet>().Init(direction);
    }
}
