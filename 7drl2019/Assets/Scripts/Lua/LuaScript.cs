using MoonSharp.Interpreter;
using System.Collections;
using Coroutine = MoonSharp.Interpreter.Coroutine;

// represents an runnable piece of Lua, usually from an event field
public class LuaScript {
    
    protected LuaContext context;

    public Coroutine scriptRoutine { get; private set;  }
    public bool done { get; private set; }

    public LuaScript(LuaContext context, string scriptString) {
        this.context = context;

        string fullScript = "return function()\n" + scriptString + "\nend";
        scriptRoutine = context.CreateScript(fullScript);
    }

    public LuaScript(LuaContext context, DynValue function) {
        this.context = context;
        scriptRoutine = context.lua.CreateCoroutine(function).Coroutine;
    }

    public IEnumerator RunRoutine() {
        done = false;
        yield return context.RunRoutine(this);
        done = true;
    }
}
