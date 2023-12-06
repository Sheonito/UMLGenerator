using System;
using System.Collections.Generic;

public class ScriptInfo
{
    public List<ScriptInfo> referenceScriptInfos = new List<ScriptInfo>();
    public Type scriptType;
    public string scriptName;
    public bool isReferenced;
    public bool isMonoScript;
}
