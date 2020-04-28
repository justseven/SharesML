using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesML.MLModel
{
    public class RegressionExperimentProgressHandler : IProgress<RunDetail<RegressionMetrics>>
    {
        private int _iterationIndex;

        public void Report(RunDetail<RegressionMetrics> iterationResult)
        {
            _iterationIndex++;
            //Console.WriteLine($"Report index:{_iterationIndex},TrainerName:{iterationResult.TrainerName},RuntimeInSeconds:{iterationResult.RuntimeInSeconds}");
        }
    }
}
