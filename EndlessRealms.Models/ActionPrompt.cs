using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;
public class ActionPrompt
{
    public class ActionPromptInfo
    {
        public string Content { get; set; } = null!;
        public string ApiCallSettingName { get; set; } = null!;
    }
    public ActionPromptInfo CREATE_WORLD { get; set; } = null!;
 
    public ActionPromptInfo CREATE_REGIONS { get; set; } = null!;

    public ActionPromptInfo CREATE_SCENE { get; set; } = null!;

    public ActionPromptInfo TALK_TO_CHARACTOR { get; set; } = null!;
    public ActionPromptInfo TALK_TO_THING { get; set; } = null!;

    public ActionPromptInfo PERFORM_ACTION_ON { get; set; } = null!;
    public ActionPromptInfo LANGUAGE_ANALYSIS { get; set; } = null!;        
}
