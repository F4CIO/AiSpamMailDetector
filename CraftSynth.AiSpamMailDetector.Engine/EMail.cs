using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace CraftSynth.AiSpamMailDetector.Engine;

public class Email
{
    [LoadColumn(0)]
    public string Content { get; set; }

    [LoadColumn(1), ColumnName("Label")]
    public bool IsSpam { get; set; }
}
