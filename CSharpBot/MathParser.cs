using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpBot
{
    public class MathParser
    {
        dynamic _sctl;

        public MathParser(CSharpBot bot)
        {
            if (Environment.OSVersion.Platform.ToString().ToLower().Contains("win"))
            {
                _sctl = new MSScriptControl.ScriptControl();
                //_sctl.AllowUI = false;
                _sctl.Language = "VBScript";
            }
            else
            {
                bot.Functions.Log("WARNING: You will not be able to use the !math command, since COM object are not supported on your platform.");
            }
        }

        public void AddVariable(string name, short value)
        {
            ((MSScriptControl.ScriptControl)_sctl).AddObject(name, value, true);
        }
        public void AddVariable(string name, ulong value)
        {
            ((MSScriptControl.ScriptControl)_sctl).AddObject(name, value, true);
        }
        public void AddVariable(string name, long value)
        {
            ((MSScriptControl.ScriptControl)_sctl).AddObject(name, value, true);
        }
        public void AddVariable(string name, double value)
        {
            ((MSScriptControl.ScriptControl)_sctl).AddObject(name, value, true);
        }
        public void AddVariable(string name, float value)
        {
            ((MSScriptControl.ScriptControl)_sctl).AddObject(name, value, true);
        }
        public void AddVariable(string name, int value)
        {
            ((MSScriptControl.ScriptControl)_sctl).AddObject(name, value, true);
        }

        public string SelfTestCurrentAction = "";

        public void SelfTest()
        {
            SelfTestCurrentAction = "Converting instance";
            MSScriptControl.ScriptControl sc = ((MSScriptControl.ScriptControl)_sctl);
            SelfTestCurrentAction = "Reset";
            sc.Reset();
            SelfTestCurrentAction = "Eval +";
            object o = sc.Eval("1 + 2");
            SelfTestCurrentAction = "Eval * /";
            o = sc.Eval("1 * 2 / 3");
        }

        public void Reset()
        {
            _sctl.Reset();
        }

        public string Calculate(string expression)
        {
            object result = _sctl.Eval(expression);
            //if (result == null)
            //    throw new Exception("Could not calculate that expression.");
            return result.ToString();
        }
    }
}
