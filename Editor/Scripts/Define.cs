using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UMLAutoGenerator
{
    public class UMLNodeView
    {
        public static readonly string k_border = "node-border";
        public static readonly string k_divider = "divider";
        public static readonly string k_labelDependencyInfo = "label-dependencyInfo";
    }
    
    public class UMLGeneratorView
    {
        public static readonly string k_toolbar = "Toolbar";
        public static readonly string k_toolbarMenu = "toolbar-menu";
        public static readonly string k_umlViewPage = "umlView-page";
        public static readonly string k_menuPage = "menu-page";
        public static readonly string k_folderObjectField = "FolderObjectField";
        public static readonly string k_buttonAdd = "Button-add";
        public static readonly string k_buttonExecute = "Button-execute";
        public static readonly string k_viewOptionDropDown = "dropDown-viewOption";
        public static readonly string k_viewOptionLabel = "label-viewOption";
        public static readonly string k_consoleView = "console-view";
        public static readonly string k_buttonConsole = "button-console";
        public static readonly string k_consoleTextArea = "console-area";
        public static readonly string k_consoleTextFieldGPT = "textField-consoleGPT";
        public static readonly string k_textfieldGptApiKey = "textfield-gptApiKey";
        public static readonly string k_buttonGptApiApply = "button-gptApiApply";
        public static readonly string k_labelGptApiKeyLog = "label-gptApiKeyLog";
    }

    public enum DependencyType
    {
        None = -1,
        Parent,
        Child,
        Referring,
        Referenced,
        ReferringAndReferenced,
    }
}


