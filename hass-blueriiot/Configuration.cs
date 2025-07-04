﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace HMX.HASSBlueriiot
{
    public class Configuration
    {
        // Boolean
        public static bool GetConfiguration(IConfigurationRoot configuration, string strVariable, out bool bConfiguration)
        {
            return GetConfiguration(configuration, strVariable, out bConfiguration, false, false);
        }

        public static bool GetOptionalConfiguration(IConfigurationRoot configuration, string strVariable, out bool bConfiguration)
        {
            return GetConfiguration(configuration, strVariable, out bConfiguration, false, true);
        }

        public static bool GetPrivateConfiguration(IConfigurationRoot configuration, string strVariable, out bool bConfiguration)
        {
            return GetConfiguration(configuration, strVariable, out bConfiguration, true, false);
        }

        private static bool GetConfiguration(IConfigurationRoot configuration, string strVariable, out bool bConfiguration, bool bPrivate, bool bOptional)
        {
            string strTemp;

            Logging.WriteLog("Configuration.GetConfiguration() Read {0}", strVariable);

            bConfiguration = false;

            if ((configuration[strVariable] ?? "") != "")
            {
                strTemp = configuration[strVariable];

                if (!bool.TryParse(strTemp, out bConfiguration))
                {
                    bConfiguration = false;

                    Logging.WriteLog("Service.Start()", "Missing configuration: {0}.", strVariable);

                    return false;
                }

                if (bPrivate)
                    Logging.WriteLog("{0}: *******", strVariable);
                else
                    Logging.WriteLog("{0}: {1}", strVariable, bConfiguration);

                return true;
            }
            else if (bOptional)
            {
                return true;
            }
            else
            {
                Logging.WriteLog("Service.Start()", "Missing configuration: {0}.", strVariable);

                return false;
            }
        }

        // String
        public static bool GetConfiguration(IConfigurationRoot configuration, string strVariable, out string strConfiguration)
        {
            return GetConfiguration(configuration, strVariable, out strConfiguration, false, false);
        }

        public static bool GetOptionalConfiguration(IConfigurationRoot configuration, string strVariable, out string strConfiguration)
        {
            return GetConfiguration(configuration, strVariable, out strConfiguration, false, true);
        }

        public static bool GetPrivateConfiguration(IConfigurationRoot configuration, string strVariable, out string strConfiguration)
        {
            return GetConfiguration(configuration, strVariable, out strConfiguration, true, false);
        }

        public static bool GetPrivateOptionalConfiguration(IConfigurationRoot configuration, string strVariable, out string strConfiguration)
        {
            return GetConfiguration(configuration, strVariable, out strConfiguration, true, true);
        }

        private static bool GetConfiguration(IConfigurationRoot configuration, string strVariable, out string strConfiguration, bool bPrivate, bool bOptional)
        {
            Logging.WriteLog("Configuration.GetConfiguration() Read {0}", strVariable);

            strConfiguration = "";

            if ((configuration[strVariable] ?? "") != "")
            {
                strConfiguration = configuration[strVariable];

                if (bPrivate)
                    Logging.WriteLog("{0}: *******", strVariable);
                else
                    Logging.WriteLog("{0}: {1}", strVariable, strConfiguration);

                return true;
            }
            else if (bOptional)
            {
                return true;
            }
            else
            {
                Logging.WriteLog("Service.Start()", "Missing configuration: {0}.", strVariable);

                return false;
            }
        }

        // Integer
        public static bool GetConfiguration(IConfigurationRoot configuration, string strVariable, out int iConfiguration)
        {
            return GetConfiguration(configuration, strVariable, out iConfiguration, false);
        }

        public static bool GetPrivateConfiguration(IConfigurationRoot configuration, string strVariable, out int iConfiguration)
        {
            return GetConfiguration(configuration, strVariable, out iConfiguration, true);
        }

        public static bool GetOptionalConfiguration(IConfigurationRoot configuration, string strVariable, out int iConfiguration)
        {
            return GetConfiguration(configuration, strVariable, out iConfiguration, false, true);
        }

        private static bool GetConfiguration(IConfigurationRoot configuration, string strVariable, out int iConfiguration, bool bPrivate, bool bOptional = false)
        {
            string strTemp;

            Logging.WriteLog("Configuration.GetConfiguration() Read {0}", strVariable);

            if ((configuration[strVariable] ?? "") != "")
            {
                strTemp = configuration[strVariable];

                if (!int.TryParse(strTemp, out iConfiguration))
                {
                    iConfiguration = 0;

                    Logging.WriteLog("Service.Start()", "Missing configuration: {0}.", strVariable);

                    return false;
                }

                if (bPrivate)
                    Logging.WriteLog("{0}: *******", strVariable);
                else
                    Logging.WriteLog("{0}: {1}", strVariable, iConfiguration);

                return true;
            }
            else if (bOptional)
            {
                iConfiguration = 0;
                return true;
            }
            else
            {
                iConfiguration = 0;

                Logging.WriteLog("Service.Start()", "Missing configuration: {0}.", strVariable);

                return false;
            }
        }

        // Integer array
        public static bool GetOptionalConfiguration(IConfigurationRoot configuration, string strVariable, out int[] iConfigurations)
        {
            Logging.WriteLog("Configuration.GetConfiguration() Read {0}", strVariable);

            var section = configuration.GetSection(strVariable);
            List<int> values = new List<int>();

            if (section.Exists() && section.GetChildren().Any())
            {
                foreach (var child in section.GetChildren())
                {
                    if (int.TryParse(child.Value, out int val))
                        values.Add(val);
                }
            }
            else if (!string.IsNullOrEmpty(configuration[strVariable]))
            {
                var parts = configuration[strVariable].Split(',');
                foreach (var part in parts)
                {
                    if (int.TryParse(part.Trim(), out int val))
                        values.Add(val);
                }
            }

            iConfigurations = values.ToArray();
            Logging.WriteLog("{0}: {1}", strVariable, string.Join(",", iConfigurations));
            return true;
        }
    }
}
