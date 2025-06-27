using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace HMX.HASSBlueriiot
{
    public class ServiceCore
    {
                private static string _strServiceName = "hass-blueriiot";
                private static string _strServiceDescription = "Blueriiot Pool Sensor";
                private static string _strConfigFile = "/data/options.json";
                private static string _strDeviceId = string.Empty;
                private static int[] _poolIndices = new int[0];

		public static void Start()
        {
			IHost webHost;
			IConfigurationRoot configuration;
                        string strMQTTUser, strMQTTPassword, strMQTTBroker;
                        string strUser, strPassword, strDeviceId;
                        int[] iPoolIndices;
                        bool bMQTTTLS;

			Logging.WriteLog("ServiceCore.Start() Built: {0}", Properties.Resources.BuildDate);

			// Load Configuration
			try
			{
				configuration = new ConfigurationBuilder().AddJsonFile(_strConfigFile, false, true).Build();
			}
			catch (Exception eException)
			{
				Logging.WriteLogError("Service.Start()", eException, "Unable to build configuration instance.");
				return;
			}

			if (!Configuration.GetConfiguration(configuration, "BlueriiotUser", out strUser))
				return;

			if (!Configuration.GetPrivateConfiguration(configuration, "BlueriiotPassword", out strPassword))
				return;

			Configuration.GetOptionalConfiguration(configuration, "MQTTUser", out strMQTTUser);
			Configuration.GetPrivateOptionalConfiguration(configuration, "MQTTPassword", out strMQTTPassword);
			if (!Configuration.GetConfiguration(configuration, "MQTTBroker", out strMQTTBroker))
				return;
                        Configuration.GetOptionalConfiguration(configuration, "MQTTTLS", out bMQTTTLS);
                        Configuration.GetOptionalConfiguration(configuration, "PoolIndex", out iPoolIndices);
                        Configuration.GetOptionalConfiguration(configuration, "DeviceId", out strDeviceId);

                        if (iPoolIndices.Length == 0)
                                iPoolIndices = new int[] { 0 };

                        if (string.IsNullOrEmpty(strDeviceId))
                                strDeviceId = (iPoolIndices.Length == 1 && iPoolIndices[0] != 0 ? string.Format("{0}-{1}", _strServiceName, iPoolIndices[0]) : _strServiceName);

                        _strDeviceId = strDeviceId;

                        MQTT.StartMQTT(strMQTTBroker, bMQTTTLS, _strServiceName, strMQTTUser, strMQTTPassword);
                        _poolIndices = iPoolIndices;
                        BlueRiiot.Start(strUser, strPassword, iPoolIndices);

			MQTTRegister();

			try
			{
				webHost = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<ASPNETCoreStartup>();
				}).Build();
			}
			catch (Exception eException)
			{
				Logging.WriteLogError("ServiceCore.Start()", eException, "Unable to build Kestrel instance.");
				return;
			}

			webHost.Run();

			Logging.WriteLog("ServiceCore.Start() Started");
		}

                public static void MQTTRegister()
                {
                        Logging.WriteLog("ServiceCore.MQTTRegister()");

                        foreach (int index in _poolIndices)
                        {
                                string baseId = string.Format("blueriiot-{0}-{1}", _strDeviceId, index);

                                MQTT.SendMessage($"homeassistant/sensor/blueriiot/sensor_pool_{index}_temperature/config",
                                        "{{\"name\":\"Pool Temperature\",\"unique_id\":\"{1}-0\",\"device\":{{\"identifiers\":[\"{1}\"],\"name\":\"{2}\",\"model\":\"Container\",\"manufacturer\":\"Blueriiot\"}},\"state_topic\":\"sensor_pool/{0}/temperature\",\"device_class\":\"temperature\",\"unit_of_measurement\":\"°C\",\"availability_topic\":\"{3}/status\"}}",
                                        index, baseId, _strServiceDescription, _strServiceName.ToLower());

                                MQTT.SendMessage($"homeassistant/sensor/blueriiot/sensor_pool_{index}_ph/config",
                                        "{{\"name\":\"Pool pH\",\"unique_id\":\"{1}-1\",\"device\":{{\"identifiers\":[\"{1}\"],\"name\":\"{2}\",\"model\":\"Container\",\"manufacturer\":\"Blueriiot\"}},\"state_topic\":\"sensor_pool/{0}/ph\",\"suggested_display_precision\":\"1\",\"unit_of_measurement\":\"pH\",\"availability_topic\":\"{3}/status\"}}",
                                        index, baseId, _strServiceDescription, _strServiceName.ToLower());

                                MQTT.SendMessage($"homeassistant/sensor/blueriiot/sensor_pool_{index}_orp/config",
                                        "{{\"name\":\"Pool Orp\",\"unique_id\":\"{1}-2\",\"device\":{{\"identifiers\":[\"{1}\"],\"name\":\"{2}\",\"model\":\"Container\",\"manufacturer\":\"Blueriiot\"}},\"state_topic\":\"sensor_pool/{0}/orp\",\"unit_of_measurement\":\"mV\",\"availability_topic\":\"{3}/status\"}}",
                                        index, baseId, _strServiceDescription, _strServiceName.ToLower());

                                MQTT.SendMessage($"homeassistant/sensor/blueriiot/sensor_pool_{index}_salinity/config",
                                        "{{\"name\":\"Pool Salinity\",\"unique_id\":\"{1}-3\",\"device\":{{\"identifiers\":[\"{1}\"],\"name\":\"{2}\",\"model\":\"Container\",\"manufacturer\":\"Blueriiot\"}},\"state_topic\":\"sensor_pool/{0}/salinity\",\"unit_of_measurement\":\"ppm\",\"availability_topic\":\"{3}/status\"}}",
                                        index, baseId, _strServiceDescription, _strServiceName.ToLower());

                                MQTT.SendMessage($"homeassistant/sensor/blueriiot/sensor_pool_{index}_conductivity/config",
                                        "{{\"name\":\"Pool Conductivity\",\"unique_id\":\"{1}-4\",\"device\":{{\"identifiers\":[\"{1}\"],\"name\":\"{2}\",\"model\":\"Container\",\"manufacturer\":\"Blueriiot\"}},\"state_topic\":\"sensor_pool/{0}/conductivity\",\"unit_of_measurement\":\"μS\",\"availability_topic\":\"{3}/status\"}}",
                                        index, baseId, _strServiceDescription, _strServiceName.ToLower());

                                MQTT.SendMessage($"homeassistant/sensor/blueriiot/sensor_pool_{index}_name/config",
                                        "{{\"name\":\"Pool Name\",\"unique_id\":\"{1}-5\",\"device\":{{\"identifiers\":[\"{1}\"],\"name\":\"{2}\",\"model\":\"Container\",\"manufacturer\":\"Blueriiot\"}},\"state_topic\":\"sensor_pool/{0}/name\",\"availability_topic\":\"{3}/status\"}}",
                                        index, baseId, _strServiceDescription, _strServiceName.ToLower());
                        }

                }

		public static void Stop()
        {
            Logging.WriteLog("ServiceCore.Stop()");

			MQTT.StopMQTT();

			Logging.WriteLog("ServiceCore.Stop() Stopped");
		}
	}
}
