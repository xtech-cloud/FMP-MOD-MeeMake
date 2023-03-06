using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using LibMVCS = XTC.FMP.LIB.MVCS;

namespace MeeX.XMA
{
    public class BlocklyInterpreter
    {
        public class LuaAgent
        {
            public LuaEnv env;
            public LuaTable table;
            public Action funcRun;
            public Action funcUpdate;
            public string code;
        }
        public LibMVCS.Logger logger { get; set; }
        public MeeX.MeeMake.StoryAPI apiStory { get; set; }
        public MeeX.MeeMake.CoreAPI apiCore { get; set; }
        private Dictionary<string, string> modules = new Dictionary<string, string>();
        private Dictionary<string, LuaAgent> luaAgents = new Dictionary<string, LuaAgent>();
        private string language = "";

        public void Initialize(GameObject _spaceRoot)
        {

        }

        public void Release()
        {
            foreach (LuaAgent luaAgent in luaAgents.Values)
            {
                try
                {
                    luaAgent.table.Dispose();
                    luaAgent.env.Dispose();
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            luaAgents.Clear();
            modules.Clear();
        }

        public void Setup()
        {

        }

        public void Update()
        {
            foreach (LuaAgent luaAgent in luaAgents.Values)
            {
                luaAgent.env.Tick();
                if (null == luaAgent.funcUpdate)
                    continue;
                try
                {
                    luaAgent.funcUpdate();
                }
                catch (System.Exception _ex)
                {
                    Debug.LogException(_ex);
                }
            }
        }


        public void AddModule(string _name, string _code)
        {
            string filename = _name;
            if (filename.EndsWith(".lua"))
                filename = filename.Substring(0, filename.Length - ".lua".Length);
            modules[filename] = _code;
        }

        public void AddScript(string _name, string _code)
        {
            LuaAgent luaAgent = new LuaAgent();
            luaAgent.code = _code;
            luaAgent.env = new LuaEnv();
            luaAgent.env.Global.Set<string, string>("G_Language", language);
            luaAgent.env.Global.Set<string, LibMVCS.Logger>("G_Logger", logger);
            luaAgent.env.Global.Set<string, MeeX.MeeMake.StoryAPI>("G_API_Story", apiStory);
            luaAgent.env.Global.Set<string, MeeX.MeeMake.CoreAPI>("G_API_Core", apiCore);
            luaAgent.table = luaAgent.env.NewTable();
            LuaTable meta = luaAgent.env.NewTable();
            meta.Set("__index", luaAgent.env.Global);
            luaAgent.table.SetMetaTable(meta);
            meta.Dispose();
            luaAgent.env.AddLoader(this.customLoader);
            luaAgents[_name] = luaAgent;
        }

        public void UseLanguage(string _language)
        {
            language = _language;
        }

        public void Execute(StoryModel.Story _story)
        {
            foreach (string name in luaAgents.Keys)
            {
                if (!name.StartsWith(_story.UUID))
                    continue;

                LuaAgent luaAgent = luaAgents[name];
                try
                {
                    luaAgent.env.DoString(luaAgent.code, "LuaBehaviour", luaAgent.table);
                    luaAgent.funcRun = luaAgent.table.Get<Action>("run");
                    luaAgent.funcUpdate = luaAgent.table.Get<Action>("update");
                }
                catch (System.Exception _ex)
                {
                    Debug.LogException(_ex);
                }
            }
        }

        public void InvokeRun(string _blockly)
        {
            UnityEngine.Debug.Log(string.Format("Invoke blockly: {0}", _blockly));
            foreach (string name in luaAgents.Keys)
            {
                if (!name.Contains(_blockly))
                    continue;

                if (null == luaAgents[name].funcRun)
                    continue;

                UnityEngine.Debug.Log("DoString: run()");
                try
                {
                    luaAgents[name].funcRun();
                }
                catch (System.Exception _ex)
                {
                    Debug.LogException(_ex);
                }
            }
        }

        private byte[] customLoader(ref string _filename)
        {
            string filename = _filename.Replace("svm/", "");
            if (!modules.ContainsKey(filename))
                return null;
            return System.Text.Encoding.UTF8.GetBytes(modules[filename]);
        }
    }
}