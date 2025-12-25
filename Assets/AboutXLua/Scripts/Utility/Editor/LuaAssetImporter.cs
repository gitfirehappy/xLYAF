using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

// 新版本ScriptedImporter实现，处理.lua文件为TextAsset
[ScriptedImporter(1, "lua")]
public class LuaAssetImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        // 读取文件内容
        byte[] fileData = System.IO.File.ReadAllBytes(ctx.assetPath);
        string textContent = System.Text.Encoding.UTF8.GetString(fileData);

        // 创建TextAsset并添加到导入上下文
        TextAsset textAsset = new TextAsset(textContent);
        ctx.AddObjectToAsset("main", textAsset);
        ctx.SetMainObject(textAsset);
    }
}