using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace CraftSynth.AiSpamMailDetector.Engine;
public class SpamPrediction
{
    [ColumnName("PredictedLabel")]
    public bool IsSpam { get; set; }
}
