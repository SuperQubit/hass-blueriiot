﻿using MBW.Client.BlueRiiotApi;
using MBW.Client.BlueRiiotApi.Builder;
using MBW.Client.BlueRiiotApi.RequestsResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using MBW.Client.BlueRiiotApi.Objects;

namespace HMX.HASSBlueriiot
{
    public class BlueRiiot
    {
        private static BlueClient? _blueClient = null;
        private static Timer? _timerPoll;
        private static int[] _poolIndices = new int[0];

        public static void Start(string strUser, string strPassword, int[] poolIndices)
        {
            Logging.WriteLog("BlueRiiot.Start()");
            _poolIndices = poolIndices;

            _blueClient = new BlueClientBuilder()
                .UseUsernamePassword(strUser, strPassword)
                .Build();

            _timerPoll = new Timer(Run, null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(30));
        }

        public static async void Run(object? oState)
        {
            SwimmingPoolGetResponse bluePools;
            SwimmingPoolLastMeasurementsGetResponse blueMeasurements;
            SwimmingPoolBlueDevicesGetResponse blueDevices;
            string strPoolId, strDeviceId;
            DateTime dtLastUpdate;
            long lRequestId = RequestManager.GetRequestId();
            double dblTemperatureCelsius = 0, dblPh = 0, dblOrp = 0, dblSalinity = 0, dblConductivity = 0;
            int iValidMeasurements = 0;
            TimeSpan tsLatency;

            Logging.WriteLog("BlueRiiot.Run() [0x{0}]", lRequestId.ToString("X8"));

    

            try
            {
                bluePools = await _blueClient.GetSwimmingPools();

                if (bluePools.Data.Count == 0)
                {
                    Logging.WriteLog("BlueRiiot.Run() [0x{0}] No pools available.", lRequestId.ToString("X8"));
                    return;
                }

                if (bluePools.Data.Count > 1)
                {
                    Logging.WriteLog("BlueRiiot.Run() [0x{0}] Multiple pools available.", lRequestId.ToString("X8"));

                    for (int iIndex = 0; iIndex < bluePools.Data.Count; iIndex++)
                        Logging.WriteLog("BlueRiiot.Run() [0x{0}] Pool \"{1}\" ({2}) found.", lRequestId.ToString("X8"), bluePools.Data[iIndex].Name, bluePools.Data[iIndex].SwimmingPoolId);
                }

                foreach (int index in _poolIndices)
                {
                    if (index >= bluePools.Data.Count)
                    {
                        Logging.WriteLog("BlueRiiot.Run() [0x{0}] PoolIndex {1} out of range, skipping.", lRequestId.ToString("X8"), index);
                        continue;
                    }

                    Logging.WriteLog("BlueRiiot.Run() [0x{0}] Pool \"{1}\" ({2}) selected.", lRequestId.ToString("X8"), bluePools.Data[index].Name, bluePools.Data[index].SwimmingPoolId);
                    MQTT.SendMessage($"sensor_pool/{index}/name", bluePools.Data[index].Name);

                    strPoolId = bluePools.Data[index].SwimmingPool.SwimmingPoolId;

                    try
                    {
                        blueDevices = await _blueClient.GetSwimmingPoolBlueDevices(strPoolId);

                        if (blueDevices.Data.Count == 0)
                        {
                            Logging.WriteLog("BlueRiiot.Run() [0x{0}] No blue devices available.", lRequestId.ToString("X8"));
                            continue;
                        }

                        strDeviceId = blueDevices.Data[0].BlueDeviceSerial;
                    }
                    catch (Exception eException)
                    {
                        Logging.WriteLogError("BlueRiiot.Run()", lRequestId, eException, "Unable to enumerate blue devices.");
                        continue;
                    }

                    try
                    {
                        blueMeasurements = await _blueClient.GetBlueLastMeasurements(strPoolId, strDeviceId);

                        dtLastUpdate = blueMeasurements.LastBlueMeasureTimestamp.Value.ToLocalTime();

                        iValidMeasurements = 0;

                        foreach (SwpLastMeasurements measurement in blueMeasurements.Data)
                        {
                            switch (measurement.Name)
                            {
                                case "temperature":
                                    if (!double.TryParse(measurement.Value.ToString(), out dblTemperatureCelsius))
                                        Logging.WriteLog("BlueRiiot.Run() [0x{0}] Invalid temperature: {1}", lRequestId.ToString("X8"), measurement.Value.ToString());
                                    else
                                    {
                                        MQTT.SendMessage($"sensor_pool/{index}/temperature", dblTemperatureCelsius.ToString());
                                        iValidMeasurements++;
                                    }
                                    break;

                                case "orp":
                                    if (!double.TryParse(measurement.Value.ToString(), out dblOrp))
                                        Logging.WriteLog("BlueRiiot.Run() [0x{0}] Invalid orp: {1}", lRequestId.ToString("X8"), measurement.Value.ToString());
                                    else
                                    {
                                        MQTT.SendMessage($"sensor_pool/{index}/orp", dblOrp.ToString());
                                        iValidMeasurements++;
                                    }
                                    break;

                                case "ph":
                                    if (!double.TryParse(measurement.Value.ToString(), out dblPh))
                                        Logging.WriteLog("BlueRiiot.Run() [0x{0}] Invalid ph: {1}", lRequestId.ToString("X8"), measurement.Value.ToString());
                                    else
                                    {
                                        MQTT.SendMessage($"sensor_pool/{index}/ph", dblPh.ToString());
                                        iValidMeasurements++;
                                    }
                                    break;

                                case "salinity":
                                    if (!double.TryParse(measurement.Value.ToString(), out dblSalinity))
                                        Logging.WriteLog("BlueRiiot.Run() [0x{0}] Invalid salinity: {1}", lRequestId.ToString("X8"), measurement.Value.ToString());
                                    else
                                    {
                                        dblSalinity *= 1000;
                                        MQTT.SendMessage($"sensor_pool/{index}/salinity", dblSalinity.ToString());
                                        iValidMeasurements++;
                                    }
                                    break;

                                case "conductivity":
                                    if (!double.TryParse(measurement.Value.ToString(), out dblConductivity))
                                        Logging.WriteLog("BlueRiiot.Run() [0x{0}] Invalid conductivity: {1}", lRequestId.ToString("X8"), measurement.Value.ToString());
                                    else
                                    {
                                        MQTT.SendMessage($"sensor_pool/{index}/conductivity", dblConductivity.ToString());
                                        iValidMeasurements++;
                                    }
                                    break;
                            }
                        }

                        if (iValidMeasurements > 0)
                        {
                            tsLatency = DateTime.Now - dtLastUpdate;
                            Logging.WriteLog("BlueRiiot.Run() [0x{0}] Current Latency: {1} minute(s)", lRequestId.ToString("X8"), tsLatency.TotalMinutes.ToString("N1"));
                        }
                    }
                    catch (Exception eException)
                    {
                        Logging.WriteLogError("BlueRiiot.Run()", lRequestId, eException, "Unable to retrieve last measurements.");
                    }
                }
            }
            catch (Exception eException)
            {
                Logging.WriteLogError("BlueRiiot.Run()", lRequestId, eException, "Unable to enumerate swimming pools.");
            }
        }
    }
}
