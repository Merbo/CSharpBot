using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSScriptControl;

namespace CSharpBot
{
    public class MathParser
    {
        ScriptControl _sctl;

        public MathParser()
        {
            _sctl = new ScriptControl();
            //_sctl.AllowUI = false;
            _sctl.Language = "VBScript";
        }

        public void AddVariable(string name, dynamic value)
        {
            _sctl.AddObject(name, value);
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
