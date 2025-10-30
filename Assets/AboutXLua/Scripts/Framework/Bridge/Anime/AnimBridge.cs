using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using XLua;

public class AnimBridge : MonoBehaviour,IBridge
{
    public AnimatorDriver.ControlMode mode = AnimatorDriver.ControlMode.GraphDriven;
    
    private IAnimationDriver driver;
    
    public async Task InitializeAsync(LuaTable luaInstance)
    {
        driver = new AnimatorDriver(mode);
        driver.Initialize(gameObject);
        
        await Task.CompletedTask;
    }
    
    public void Play(string name) => driver?.Play(name);
    public void Stop() => driver?.Stop();
    public void SetFloat(string name, float value) => driver?.SetFloat(name, value);
    public void SetBool(string name, bool value) => driver?.SetBool(name, value);
    public void SetTrigger(string name) => driver?.SetTrigger(name);
    public void SetInt(string name, int value) => driver?.SetInt(name, value);
}
