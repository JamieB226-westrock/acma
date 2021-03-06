﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lithnet.Logging;
using Lithnet.Acma;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Lithnet.Common.ObjectModel;
using System.Transactions;

namespace Lithnet.Acma.TestEngine
{
    [DataContract(Name = "acma-unit-tests", Namespace = "http://lithnet.local/Lithnet.Acma/v1/")]
    public class UnitTestFile : UINotifyPropertyChanges, IExtensibleDataObject
    {
        public UnitTestFile()
        {
            this.Initialize();
        }

        public static bool BreakOnTestFailure { get; set; }

        [DataMember(Name = "config-version", Order = 0)]
        public string ConfigVersion { get; set; }

        [DataMember(Name = "description", Order = 1)]
        public string Description { get; set; }

        [DataMember(Name = "unit-tests", Order = 5)]
        public ObservableCollection<UnitTestObject> UnitTestObjects { get; set; }

        public string FileName { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }

        public UnitTestOutcomes Execute()
        {
            UnitTestOutcomes results = new UnitTestOutcomes();
            results.StartTime = DateTime.Now;
            results.Server = ActiveConfig.DB.ServerName;
            results.ConfigFile = ActiveConfig.XmlConfig.FileName;
            UniqueValueConstructor.DisableCachingGlobal = true;

            foreach (UnitTestObject item in this.UnitTestObjects)
            {
                try
                {
                    if (item is UnitTest)
                    {
                        results.Add(((UnitTest)item).Execute());
                    }
                    else if (item is UnitTestGroup)
                    {
                        results.AddRange(((UnitTestGroup)item).Execute());
                    }
                    else
                    {
                        throw new InvalidOperationException("The unit test object type was unknown");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    throw new UnitTestEngineException("Unexpected error in the unit test engine", ex);
                }
            }

            results.EndTime = DateTime.Now;
            return results;
        }

        private void Initialize()
        {
            this.UnitTestObjects = new ObservableCollection<UnitTestObject>();
        }

        private void OnDeserializing(StreamingContext context)
        {
            this.Initialize();
        }

        public static UnitTestFile LoadXml(string filename)
        {
            bool canCache = ActiveConfig.DB.CanCache;

            try
            {
                UniqueIDCache.ClearIdCache(UnitTest.CacheGroupName);
                ActiveConfig.DB.CanCache = true;
                UnitTestFile configFile = Serializer.Read<UnitTestFile>(filename, "acma-unit-tests", "http://lithnet.local/Lithnet.Acma/v1/");
                configFile.FileName = filename;
                return configFile;
            }
            finally
            {
                ActiveConfig.DB.CanCache = canCache;
            }
        }
    }
}