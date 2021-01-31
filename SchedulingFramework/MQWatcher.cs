using IBM.WMQ;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SchedulingFramework
{
    public class MQWatcher
    {
        private MQQueueManager queueManager;
        private MQQueue queIn;
        private MQQueue queOut;
        private Hashtable mqProperties;
        private MQMessage mqMessage;

        /// <summary>
        /// Name of the host on which Queue manager is running
        /// </summary>
        private string hostName = ConfigurationManager.AppSettings["MQ_HOST"].ToString();

        /// <summary>
        /// Port number on which Queue manager is listening
        /// </summary>
        private string port = ConfigurationManager.AppSettings["MQ_PORT"].ToString();

        /// <summary>
        /// Name of the channel
        /// </summary>
        private string channelName = ConfigurationManager.AppSettings["MQ_CHANNEL"].ToString();

        /// <summary>
        /// Name of the Queue manager to connect to
        /// </summary>
        private string queueManagerName = ConfigurationManager.AppSettings["MQ_QUEUE_MANAGER"].ToString();

        /// <summary>
        /// Queue name.
        /// </summary>
        private string queueName = ConfigurationManager.AppSettings["MQ_QUEUE"].ToString();

        private string user = ConfigurationManager.AppSettings["MQ_USERID"].ToString();

        private string password = ConfigurationManager.AppSettings["MQ_PASSWORD"].ToString();

        private bool isAuthEnable = Convert.ToBoolean(ConfigurationManager.AppSettings["MQ_AUTHENTICATION_ENABLED"].ToString());

        public void Init()
        {
            mqProperties = new Hashtable();
            mqProperties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
            mqProperties.Add(MQC.HOST_NAME_PROPERTY, hostName);
            mqProperties.Add(MQC.PORT_PROPERTY, port);
            mqProperties.Add(MQC.CHANNEL_PROPERTY, channelName);
            mqProperties.Add(MQC.USER_ID_PROPERTY, user);
            mqProperties.Add(MQC.PASSWORD_PROPERTY, password);
            mqProperties.Add(MQC.USE_MQCSP_AUTHENTICATION_PROPERTY, isAuthEnable);

        }

        public void ReadMQMessage()
        {
            try
            {
                // get message options
                MQGetMessageOptions mqGMO = new MQGetMessageOptions();
                mqGMO.Options = MQC.MQGMO_FAIL_IF_QUIESCING + MQC.MQGMO_NO_WAIT + MQC.MQGMO_BROWSE_NEXT; // browse with no wait
                mqGMO.MatchOptions = MQC.MQMO_NONE;

                queueManager = new MQQueueManager(queueManagerName, mqProperties);
                //queIn = queueManager.AccessQueue(queueName, MQC.MQOO_FAIL_IF_QUIESCING);
                queIn = queueManager.AccessQueue(queueName, MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING);
                while (true)
                {
                    try
                    {
                        queueManager = new MQQueueManager(queueManagerName, mqProperties);
                        queIn = queueManager.AccessQueue(queueName, MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING);

                        while (true)
                        {
                            mqMessage = new MQMessage();
                            //queIn.Get(mqMessage, mqGMO);
                            queIn.Get(mqMessage);
                            string message = mqMessage.ReadString(mqMessage.MessageLength);
                            mqMessage.ClearMessage();
                            bool isFileSaved = SaveFile(message);
                        }
                    }
                    catch (MQException mqe)
                    {
                        if (queIn != null && queIn.IsOpen)
                        {
                            queIn.Close();
                        }
                        if (queueManager != null && queueManager.IsConnected)
                        {
                            queueManager.Disconnect();
                        }
                        if (mqe.ReasonCode == 2033)
                        {
                            Console.WriteLine("No Message");
                            Thread.Sleep(10000);
                        }
                        else
                        {
                            Console.WriteLine(mqe.ReasonCode + " : " + mqe.Reason);
                        }
                    }
                }
            }
            catch (MQException mqe)
            {
                Console.WriteLine("Conneciton Error: " + mqe.ReasonCode + " : " + mqe.Reason);
            }

            //Thread.Sleep(10000);
        }

        public void SendMQMessage(Schedule sch)
        {
            

            string line = CreateFile(sch);

            int openOptions = MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING;
            MQPutMessageOptions mpo = new MQPutMessageOptions();

            try
            {
                queueManager = new MQQueueManager(queueManagerName, mqProperties);
                queOut = queueManager.AccessQueue(queueManagerName, openOptions);

                //Define a simple MQ message and write testx in UTF format

                MQMessage msgSend = new MQMessage();
                msgSend.Format = MQC.MQFMT_STRING;
                msgSend.MessageType = MQC.MQMT_DATAGRAM;
                msgSend.MessageId = MQC.MQMI_NONE;
                msgSend.CorrelationId = MQC.MQMI_NONE;

                //put the message on the que

                queOut.Put(msgSend, mpo);

            }
            catch (Exception e)
            {

            }
            finally
            {
                if (queOut != null)
                {
                    queOut.Close();
                }
                if (queueManager != null)
                {
                    queueManager.Disconnect();
                }

            }
        }

        public string CreateFile(Schedule ScheduleData)
        {
            string schedule = String.Empty;

            if (ScheduleData != null)
            {

                if (ScheduleData.FlightNumber != String.Empty)
                {
                    var lenght = ScheduleData.FlightNumber.Trim().Length;
                    var flightNumber = lenght < 1000 ? "00" + ScheduleData.FlightNumber.Trim() : ScheduleData.FlightNumber.Trim();
                    schedule += flightNumber;
                }
                else
                {
                    schedule += padBlanks(4);
                }
                if (ScheduleData.PeriodOfOperation != String.Empty)
                {
                    schedule += ScheduleData.PeriodOfOperation.Trim();
                }
                else
                {
                    schedule += padBlanks(14);
                }
                if (ScheduleData.DaysOfOperation != String.Empty)
                {
                    schedule += ScheduleData.DaysOfOperation.Trim();
                }
                else
                {
                    schedule += padBlanks(7);
                }
                if (ScheduleData.DepartureTime != String.Empty)
                {
                    schedule += ScheduleData.DepartureTime.Trim();
                }
                else
                {
                    schedule += padBlanks(4);
                }
                if (ScheduleData.OriginalStation != String.Empty)
                {
                    schedule += ScheduleData.OriginalStation.Trim();
                }
                else
                {
                    schedule += padBlanks(3);
                }
                if (ScheduleData.DestinationStation != String.Empty)
                {
                    schedule += ScheduleData.DestinationStation.Trim();
                }
                else
                {
                    schedule += padBlanks(3);
                }
                if (ScheduleData.Aircraft != String.Empty)
                {
                    schedule += ScheduleData.Aircraft.Trim();
                }
                else
                {
                    schedule += padBlanks(3);
                }


            }

            return schedule;
        }

        public bool SaveFile(string message)
        {
            try
            {
                if (!Directory.Exists(ConfigurationManager.AppSettings["ARCHIVE_PATH"].ToString()))
                {
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["ARCHIVE_PATH"].ToString());
                }

                if (!Directory.Exists(Path.Combine(ConfigurationManager.AppSettings["ARCHIVE_PATH"].ToString(), DateTime.Now.ToString("MMddyyyy"))))
                {
                    Directory.CreateDirectory(Path.Combine(ConfigurationManager.AppSettings["ARCHIVE_PATH"].ToString(), DateTime.Now.ToString("MMddyyyy")));
                }

                string filePath = Path.Combine(ConfigurationManager.AppSettings["ARCHIVE_PATH"].ToString(), DateTime.Now.ToString("MMddyyyy"));
                string fileName = DateTime.Now.ToString("hhmmssms");
                string fileExtension = ".xml";
                int i = 0;
                while (File.Exists(Path.Combine(filePath, fileName + fileExtension)))
                {
                    i++;
                }
                fileName = i > 0 ? fileName + "_" + i : fileName;

                File.WriteAllText(Path.Combine(filePath, fileName + fileExtension), message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string padBlanks(int numberOfBlanks)
        {
            string padding = String.Empty;
            if (numberOfBlanks > 0)
            {
                for (int i = 0; i < numberOfBlanks; i++) padding += " ";
            }
            return padding;
        }
    }
}

