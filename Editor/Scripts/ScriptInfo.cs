using System;
using System.Collections.Generic;

public class ScriptInfo
{
    public List<ScriptInfo> referringScriptInfos = new List<ScriptInfo>(); // 참조하고 있는 ScriptInfo
    public List<ScriptInfo> referencedScriptInfos = new List<ScriptInfo>(); // 참조되고 있는 ScriptInfo
    public Type scriptType;
    public string scriptName;
    public bool isReferenced;
    public bool isMonoScript;
}
