using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using XLua;

public class XLuaManager : MonoBehaviour
{
    public static XLuaManager instance;
    public LuaEnv env { get; private set; } = new LuaEnv();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        env.AddLoader(LuaLoader);
    }

    private byte[] LuaLoader(ref string _fileName)
    {
        TextAsset script;
        byte[] result = new byte[] { };

        script = AssetBundleManager.instance.LoadResource("luascripts", $"{_fileName}.lua", typeof(TextAsset)) as TextAsset;
        result = Encoding.UTF8.GetBytes(script.text);
        return result;

        //AssetBundleManager.instance.LoadResourceAsync("luascripts", $"{_fileName}.lua", typeof(TextAsset), (luascript) =>
        //{
        //    script = luascript as TextAsset;

        //    //XLuaManager.instance.env.DoString(script.text);

        //    result = Encoding.UTF8.GetBytes(script.text);
        //});

        //Debug.Log(result.Length);

        //return result;

    }
}
