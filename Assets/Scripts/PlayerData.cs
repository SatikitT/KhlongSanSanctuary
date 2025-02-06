using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField]
    private int money = 10;
    [SerializeField]
    private int faith = 10;


    public int GetMoney()
    {
        return money;
    }

    public int GetFaith()
    {
        return faith;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log($"Money Added: {amount}, Total Money: {money}");
    }

    public void AddFaith(int amount)
    {
        faith += amount;
        Debug.Log($"Faith Added: {amount}, Total Faith: {faith}");
    }
}
