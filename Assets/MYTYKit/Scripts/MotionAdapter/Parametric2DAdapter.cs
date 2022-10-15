using MYTYKit.Controllers;
using UnityEngine;

using MYTYKit.MotionTemplates;

namespace MYTYKit.MotionAdapters
{
    public class Parametric2DAdapter : DampingAndStabilizingVec3Adapter, ITemplateObserver
    {
        public ParametricTemplate template;
        public string xParamName;
        public string yParamName;
        public MYTYController con;

        public void TemplateUpdated()
        {
            Vector2 val = new Vector2(template.GetValue(xParamName), template.GetValue(yParamName));
            AddToHistory(val);
        }
        void Update()
        {
            var input = con as IVec2Input;
            if (input == null) return;
            input.SetInput(GetResult());

        }
    }
}
