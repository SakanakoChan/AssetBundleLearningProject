using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class XLuaManager : MonoBehaviour
{
    public static XLuaManager instance;
    public LuaEnv env {  get; private set; } = new LuaEnv();

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
    }
}
