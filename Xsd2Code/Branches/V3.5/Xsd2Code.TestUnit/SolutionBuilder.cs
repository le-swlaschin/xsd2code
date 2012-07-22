using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

//using Microsoft.Build.BuildEngine;

namespace Xsd2Code.TestUnit
{
    public class SolutionBuilder
    {
        [STAThread]
        public static bool CompileSolution(string solutionName, out string errorMessage)
        {
            errorMessage = string.Empty;
            bool compilResult;
            var b = new BasicFileLogger();
            b.Parameters = Path.Combine(Path.GetTempPath(), "Compil_" + Path.GetRandomFileName());
            b.register();
            
            var loggers = new List<ILogger>();
            //loggers.Add(new ConsoleLogger());
            loggers.Add(b);

            var projectCollection = new ProjectCollection();
            projectCollection.RegisterLoggers(loggers);

            var project = projectCollection.LoadProject(solutionName); 
            try
            {
                compilResult = project.Build();
            }
            finally
            {
                projectCollection.UnregisterAllLoggers();
            }


            errorMessage = b.getLogoutput();
            errorMessage += "\n\t" + b.Warningcount + " Warnings. ";
            errorMessage += "\n\t" + b.Errorcount + " Errors. ";
            
            b.Shutdown();
            
            return compilResult;
        }
    }
}
