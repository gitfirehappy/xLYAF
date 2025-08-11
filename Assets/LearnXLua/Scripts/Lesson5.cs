using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using XLua;

public class Lesson5 : MonoBehaviour
{
    public LuaInit luaInit;
    
    static List<LuaTable> tablePool = new List<LuaTable>();
    
    void Start()
    {
        luaInit.luaEnv.DoString("require 'Lesson5'");
        luaInit.luaEnv.Global.Get<Action>("Main")?.Invoke();
    }

    public static LuaTable GetLuaTableFromPool()
    {
        if (tablePool.Count > 0)
        {
            var table = tablePool[0];
            tablePool.RemoveAt(0);
            return table;
        }

        return null;
    }

    public static void ReturnLuaTableToPool(LuaTable table)
    {
        table.Dispose();
        tablePool.Add(table);
    }
    
}

[LuaCallCSharp]
public class Person_Lesson5
{
    public string Name;
    public int Age;
    public Point Position;
    
    public Person_Lesson5(string name, int age, Point position)
    {
        Name = name;
        Age = age;
        Position = position;
    }

    public virtual void SayHello()
    {
        Debug.Log($"C# Hello,my name is {Name},age {Age},at {Position}");
    }
    
    public int GetAge() => Age;
    public void SetAge(int age) => Age = age;
}

[LuaCallCSharp]
public struct Point
{
    public float X;
    public float Y;
    public Point(float x, float y)
    {
        X = x;
        Y = y;
    }
    public override string ToString() => $"({X},{Y})";
}

[CSharpCallLua]
public interface IPersonAction
{
    void OnMove(Point newPos);
}
