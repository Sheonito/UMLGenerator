using System.Collections.Generic;

public class ScriptInfo
{
    public List<ScriptInfo> referenceScriptInfos = new List<ScriptInfo>();
    public string scriptName;
    public bool isReferenced;
}
