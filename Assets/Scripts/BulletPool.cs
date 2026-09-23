using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField]
    GameObject BulletPrefab;
    [SerializeField]
    private ushort ObjectPoolQuantity;

    private Stack<Bullet> bullets;
    private Stack<Bullet> RollbackBullets;
    //private Stack<Bullet> bulletsToBeRemoved;
    //private Stack<Bullet> bulletsToBeAdded;

    private void Start()
    {
        bullets = new Stack<Bullet>(ObjectPoolQuantity);
        //bulletsToBeAdded = new Stack<Bullet>(10);
        //bulletsToBeRemoved = new Stack<Bullet>(10);
        for (int i = 0; i < ObjectPoolQuantity; i++)
        {
            var bullet = Instantiate(BulletPrefab, this.transform).GetComponent<Bullet>();
            bullet.pool = this;
            bullets.Push(bullet);
        }
        RollbackBullets = new Stack<Bullet>(bullets);
    }

    public Bullet Pull()
    {
        if (bullets.Count == 0) { 
            //Debug.Log("empty"); 
            return null; }
        var bullet = bullets.Pop();
        //bulletsToBeRemoved.Push(bullet);
        //Debug.Log("shoot");
        return bullet;
    }
    public void Return(Bullet bullet)
    {
        bullets.Push(bullet);
    }
    public void RestoreState()
    {
        bullets.Clear();
        foreach (var bullet in RollbackBullets) 
        {
            bullets.Push(bullet);
        }
        //for (int i = 0; i < bulletsToBeRemoved.Count; i++)
        //{
        //    bullets.Push(bulletsToBeRemoved.Pop());
        //}
    }
    public void SaveState()
    {
        RollbackBullets.Clear();
        foreach (var bullet in bullets)
        {
            RollbackBullets.Push(bullet);
        }
        //bulletsToBeRemoved.Clear();

        //for (int i = 0; i < bulletsToBeAdded.Count; i++)
        //{
        //    bullets.Push(bulletsToBeAdded.Pop());
        //}
    }
}
