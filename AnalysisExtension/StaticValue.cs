using System.Reflection;
using System.Resources;

namespace AnalysisExtension
{
    public class StaticValue
    {
        public static ResourceManager RESOURCE_MANAGER = new ResourceManager("AnalysisExtension.String",Assembly.GetExecutingAssembly());
    }
}
