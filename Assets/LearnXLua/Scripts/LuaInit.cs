using System.IO;
using System.Text;
using UnityEngine;
using XLua;

public class LuaInit : MonoBehaviour
{
    public LuaEnv luaEnv;

    void Awake()
    {
        luaEnv = new LuaEnv();

        // 注册 Loader
        luaEnv.AddLoader((ref string filepath) =>
        {
            string fullPath = Application.dataPath + "/LearnXLua/LuaScripts/" + filepath.Replace('.', '/') + ".lua";
            if (File.Exists(fullPath))
            {
                return Encoding.UTF8.GetBytes(File.ReadAllText(fullPath));
            }
            return null;
        });
    }

    void OnDestroy()
    {
        luaEnv.Dispose();
    }
}