﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Lithnet.Acma;
using Lithnet.MetadirectoryServices;
using Lithnet.Acma.DataModel;

namespace Lithnet.Acma.PS
{
    [Cmdlet(VerbsCommon.Set, "AcmaSequence")]
    public class SetAcmaSequence : Cmdlet
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The name of the sequence",
            Position = 0,
            ParameterSetName = "IncrementingSequence",
            ValueFromPipeline = false)]
        [Parameter(
            Mandatory = true,
            HelpMessage = "The name of the sequence",
            Position = 0,
            ParameterSetName = "CyclingSequence",
            ValueFromPipeline = false)]
        public string Name { get; set; }

        [Parameter(
           Mandatory = true,
           HelpMessage = "The starting value of the sequence",
           Position = 1,
           ParameterSetName = "IncrementingSequence",
           ValueFromPipeline = false)]
        [Parameter(
            Mandatory = true,
            HelpMessage = "The starting value of the sequence",
            Position = 1,
            ParameterSetName = "CyclingSequence",
            ValueFromPipeline = false)]
        public long StartValue { get; set; }

        [Parameter(
           Mandatory = true,
           HelpMessage = "The value to increment the sequence with",
           Position = 2,
           ParameterSetName = "IncrementingSequence",
           ValueFromPipeline = false)]
        [Parameter(
            Mandatory = true,
            HelpMessage = "The value to increment the sequence with",
            Position = 2,
            ParameterSetName = "CyclingSequence",
            ValueFromPipeline = false)]
        public long IncrementBy { get; set; }

        [Parameter(
           Mandatory = true,
           HelpMessage = "The minimum value to apply to a cycling sequence",
           Position = 3,
           ParameterSetName = "CyclingSequence",
           ValueFromPipeline = false)]
        public long? MinValue { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The maximum value to apply to a cycling sequence",
            Position = 4,
            ParameterSetName = "CyclingSequence",
            ValueFromPipeline = false)]
        public long? MaxValue { get; set; }

        protected override void ProcessRecord()
        {
            Global.ThrowIfNotConnected(this);

            try
            {
                bool cycle = false;

                if (this.MinValue == null && this.MaxValue == null)
                {
                    cycle = false;
                }
                else
                {
                    if (this.MinValue == null || this.MaxValue == null)
                    {
                        throw new ArgumentException("Both MinValue and MaxValue must be specified");
                    }
                    else
                    {
                        cycle = true;
                    }
                }

                AcmaSequence sequence = ActiveConfig.DB.GetSequence(this.Name);

                sequence.Increment = this.IncrementBy;
                sequence.IsCycleEnabled = cycle;
                sequence.MaxValue = this.MaxValue;
                sequence.MinValue = this.MinValue;
                sequence.StartValue = this.StartValue;

                ActiveConfig.DB.UpdateSequence(sequence);
                ActiveConfig.DB.ClearCache();

                Console.WriteLine("Sequence updated");
            }
            catch (Exception ex)
            {
                ErrorRecord error = new ErrorRecord(ex, "UnknownError", ErrorCategory.NotSpecified, null);
                ThrowTerminatingError(error);
            }
        }
    }
}
