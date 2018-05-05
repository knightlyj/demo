using UnityEngine;
using System.Collections;
using System;
using System.IO;

public enum WeaponType
{
    Empty = 0,
    Sword = 1,
    Shield = 2,
    HugeSword = 3,
    HugeAxe = 4,
}

public class WeaponData
{
    public string upperIdle;
    public string upperIdleTwoHanded;
    public string upperMove;
    public string upperMoveTwoHanded;
}

public static class WeaponConfig
{
    enum WeaponCsvIndex
    {
        UpperIdle = 1,
        UpperIdleTwoHanded = 2,
        UpperMove = 3,
        UpperMoveTwoHanded = 4,
    }

    static WeaponData[] weaponData = null;
    static WeaponData defaultWeaponData = null;
    static WeaponConfig()
    {
        //默认武器数据
        defaultWeaponData = new WeaponData();
        defaultWeaponData.upperIdle = "Idle-Sword";
        defaultWeaponData.upperMove = "Move-Sword";

        Array a = Enum.GetValues(typeof(WeaponType));
        weaponData = new WeaponData[a.Length];

        using (FileStream fs = new FileStream(Application.streamingAssetsPath + "/weapon.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding("GB18030")))
            {
                string input;
                input = sr.ReadLine(); //第一行为标题栏
                int count = 0;
                while ((input = sr.ReadLine()) != null && count <= weaponData.Length)
                {
                    string[] cell = null;
                    cell = input.Split(',');
                    if (cell.Length >= 3) //表格长度限制
                    {
                        weaponData[count] = new WeaponData();
                        weaponData[count].upperIdle = cell[(int)WeaponCsvIndex.UpperIdle];
                        weaponData[count].upperMove = cell[(int)WeaponCsvIndex.UpperMove];
                    }
                    else
                    { //如果表格数据长度不对,则用默认数据
                        weaponData[count] = defaultWeaponData;
                    }
                    count++;
                }
                sr.Close();
            }
            fs.Close();
        }
    }

    public static WeaponData GetWeaponData(WeaponType type)
    {
        return weaponData[(int)type];
    }
}
