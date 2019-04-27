using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisExtension.ExceptionMode
{
    class ProjectNotOpenException : Exception, ISerializable
    {
        private static string msg = "not open project yet";

        public ProjectNotOpenException() : base(msg)
        {
        }
    }
}
