/**
 * Universal GPS Json Receiver example over RabbitMQ.
 *
 * Required packages:
 * https://www.nuget.org/packages/RabbitMQ.Client (v12.0.1)
 * https://www.nuget.org/packages/Newtonsoft.Json/ (v5.1.0)
 *
 * Tested Using:
 * Microsoft Visual Studio Community 2017: v15.9.6
 * NuGet Package Manager: v4.6.0
 * Microsoft .NET Framework: v4.7.03056 (projected targeted at 4.6.1)
 *
 */

#define DEBUG

// Enable the directive below if you want the example to load a GPS
// json message from file.
//#define SAMPLE_FROM_FILE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.IO;
using System.Collections;

namespace GPSReceiverExample
{
    class Program
    {
        #if SAMPLE_FROM_FILE
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify the name of the json file to decode.");
                return 1;
            }

            Console.WriteLine("-> Starting up...");

            var json = File.ReadAllText(args[0]);

            // Deserialize the JSON message into a basic class
            // called UniversalMsg so we can extract the primary
            // details before continuing.
            var msg = JsonConvert.DeserializeObject<UniversalMsg>(json);
            if (msg.message_type == "gps" && msg.message_ver == 1 && msg.valid)
            {
                // Now we can Deserialize the complete message and handle
                // it.
                Console.WriteLine("-> Received new message:");
                var gpsMsg = JsonConvert.DeserializeObject<GpsMessage>(json);
                handleGps(gpsMsg);
            } else {
                Console.WriteLine("-> Unable to decode message");
            }

            #if DEBUG
                Console.WriteLine("Press enter to close...");
                Console.ReadLine();
            #endif

            return 0;
        }
        #else
        static void Main(string[] args)
        {
            // Step 1: Create a connection factory, this will allow us to connect
            // to the remote RabbitMQ server using the credentials below.
            var factory = new ConnectionFactory()
            {
                HostName = Properties.Settings.Default.ServerHostname,
                UserName = Properties.Settings.Default.ServerUsername,
                Password = Properties.Settings.Default.ServerPassword,
                Port = Properties.Settings.Default.ServerPort,
                VirtualHost = Properties.Settings.Default.VirtualHost
            };

            Console.WriteLine("-> Connecting to Server: {0}:{1} (Virtualhost: {2})",
                Properties.Settings.Default.ServerHostname,
                Properties.Settings.Default.ServerPort,
                Properties.Settings.Default.VirtualHost);

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Step 2: We declare a new queue called "gps.receiver.test", the name is up to you.
                // If the queue does not exist yet, it will automatically be created on the
                // RabbitMQ server. If it already exists it will be used as is. Queues is
                // what keeps our messages until we read it.
                // See: https://www.rabbitmq.com/queues.html
                //
                // Note that the queue we create is "durable", meaning that data within the
                // queue will be persistent and is stored on disk and not just in memory. Data will
                // also remain in the queue if our client is not reading it or if the server reboots.
                Console.WriteLine("-> Declaring Queue: {0}",
                    Properties.Settings.Default.QueueName);

                channel.QueueDeclare(queue: Properties.Settings.Default.QueueName,
                                     durable: Properties.Settings.Default.FlagsDurable,
                                     exclusive: Properties.Settings.Default.FlagsExclusive,
                                     autoDelete: Properties.Settings.Default.FlagsAutoDelete,
                                     arguments: null);

                // Step 3: Bind our queue to an exchange. Exchanges routes messages to one
                // or more queues. We need to bind our "gps.receiver.test" queue to an existing exchange
                // called "acm.gws". Furthermore, we specify a routing/binding key called "gps.#".
                // All messages are sent using a routing key called "gps.{imei}". We are specifying
                // here that the exhange "acm.gws", should route all messages containing the "gps.*"
                // routing key to our "gps.receiver.test" queue.
                //
                // Note: The "acm.gws" exchange is setup as a "topic" exchange, which allows us to
                // route messages as explained above.
                Console.WriteLine("-> Binding Queue: {0} to Exchange: {1} using Routing Key: {2}",
                    Properties.Settings.Default.QueueName,
                    Properties.Settings.Default.ExchangeName,
                    Properties.Settings.Default.RoutingKey);

                channel.QueueBind(queue: Properties.Settings.Default.QueueName, 
                                  exchange: Properties.Settings.Default.ExchangeName, 
                                  routingKey: Properties.Settings.Default.RoutingKey);

                // Step 4: Setup a basic message consumer event handler.
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    // This callback is called whenever a new message becomes
                    // available.
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("-> Received new message:");
                    Console.WriteLine(message);

                    // Deserialize the JSON message into a basic class
                    // called UniversalMsg so we can extract the primary
                    // details before continuing.
                    var msg = JsonConvert.DeserializeObject<UniversalMsg>(message);
                    if (msg.message_type == "gps" && msg.message_ver == 1 && msg.valid)
                    {
                        // Now we can Deserialize the complete message and handle
                        // it.
                        var gpsMsg = JsonConvert.DeserializeObject<GpsMessage>(message);
                        handleGps(gpsMsg);
                    }

                    // Send an ACK back to the server that we have processed the
                    // message. We will automatically receive a new message afterwards
                    // if there is one available.
                    //
                    // IMPORTANT: If no ACK is returned to the server it will keep the
                    // message in the queue, potentially causing duplicates and queues
                    // becoming full.
                    Console.WriteLine("<- Sending Acknowledgement.");
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine("");
                };

                // Step 5: Start our message consumer
                Console.WriteLine("-> Starting consumer.");
                channel.BasicConsume(queue: Properties.Settings.Default.QueueName,
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine("");
                Console.WriteLine("Waiting for messages, press [enter] to exit...");
                Console.ReadLine();
            }
        }
        #endif

        public static void handleGps(GpsMessage msg)
        {
            Console.WriteLine("==============================================================================");
            Console.WriteLine("imei:           {0}", msg.device.imei);
            Console.WriteLine("message_type:   {0}", msg.message_type);
            Console.WriteLine("gateway:        {0}", msg.gateway);
            Console.WriteLine("port:           {0}", msg.port);
            Console.WriteLine("transmission:   {0}", msg.transmission);

            Console.WriteLine("gsm.signal_str: {0}", msg.gsm[0].signal_str);
            Console.WriteLine("gsm.status:     {0}", String.Join(", ", msg.gsm[0].status));

            Console.WriteLine("sims.msisdn:    {0}", msg.sims[0].msisdn);
            Console.WriteLine("sims.iccid:     {0}", msg.sims[0].iccid);

            Console.WriteLine("gps.timestamp:  {0}", msg.gps.timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            Console.WriteLine("gps.latitude:   {0}", msg.gps.latitude);
            Console.WriteLine("gps.longitude:  {0}", msg.gps.longitude);
            Console.WriteLine("gps.altitude:   {0}", msg.gps.altitude);
            Console.WriteLine("gps.speed:      {0}", msg.gps.speed);
            Console.WriteLine("gps.heading:    {0}", msg.gps.heading);
            Console.WriteLine("gps.satellites: {0}", msg.gps.satellites);
            Console.WriteLine("gps.activity:   {0}", msg.gps.activity);
            Console.WriteLine("gps.odometer:   {0}", msg.gps.odometer);
            Console.WriteLine("gps.trip_odo:   {0}", msg.gps.trip_odo);
            Console.WriteLine("gps.fix:        {0}", String.Join(", ", msg.gps.fix));
            Console.WriteLine("gps.dop:        hdop: {0} vdop: {1} pdop: {2} tdop: {3}", msg.gps.hdop, msg.gps.vdop, msg.gps.pdop, msg.gps.tdop);

            foreach (var msg_event in msg.events)
            {
                // The first element in the list is always the event code
                Console.WriteLine("event:          {0}", msg_event[0]);
                foreach (var event_field in msg_event)
                {
                    // Any additional elements contains event data and are defined as IList's with a
                    // count of 2 where the first element is the field name and the second element is
                    // the value, e.g.:
                    // [ "HARSH_EVENT:ACCIDENT", [ "x", 15.3 ], ["y", 20.8 ], [ "z", 17.4 ] ]
                    if (event_field is IList && event_field.Count == 2)
                    {
                        Console.WriteLine("         field: {0}={1}", event_field[0], event_field[1]);
                    }
                }
            }

            foreach (var sensor in msg.sensors)
            {
                Console.WriteLine("sensor:         {0} = {1}", sensor[0], sensor[1]);
            }

            foreach (var pid in msg.obd_ii.mode_01)
            {
                Console.WriteLine("OBD PID:        {0} = {1}", pid[0], pid[1]);
            }

            Console.WriteLine("==============================================================================");
        }

        public class UniversalMsg
        {
            /// <value>The message version, currently there is only a version 1 message.</value>
            public int message_ver { get; set; }

            /// <value>Message type, can be "register", "gps", "history", "status", "event", "heartbeat".</value>
            public string message_type { get; set; }

            /// <value>
            /// Whether the Gateway validated this as a "valid" message, i.e. anyone reading the data can use
            /// it as a valid position. It is recommended that if valid is false, that the message is either
            /// discarded, or stored but not used in any calculations/processing.
            /// </value>
            public bool valid { get; set; }
        }

        public class GpsMessage
        {
            /// <value>The JSON message structure version, currently there is only a version 1 message.</value>
            public int message_ver { get; set; }

            /// <value>Message type, can be "register", "gps", "history", "status", "event", "heartbeat".</value>
            public string message_type { get; set; }

            /// <value>The domain which received the message from the unit.</value>
            public string gateway { get; set; }

            /// <value>The TCP/UDP port that unit connected to the gateway.</value>
            public int port { get; set; }

            /// <value>Transmission type: tcp, udp, http, https, sms.</value>
            public string transmission { get; set; }

            /// <value>The ISO 8601 timestamp when the message was received at the gateway (Don't confuse with the GPS timestamp)</value>
            public DateTime timestamp { get; set; }

            /// <value>The message source, e.g. device, 3rd_party_service</value>
            public string source { get; set; }

            /// <value>Sequence number, usually provided by the unit</value>
            public int seq_no { get; set; }

            /// <value>
            /// Whether the Gateway validated this as a "valid" message, i.e. anyone reading the data can use
            /// it as a valid position. It is recommended that if valid is false, that the message is either
            /// discarded, or stored but not used in any calculations/processing.
            /// </value>
            public bool valid { get; set; }

            /// <value>Can be any of the following: "unknown", "still", "walking", "running", "driving", "parked", "idling"</value>
            public string activity { get; set; }

            public Device device { get; set; }

            public Network network { get; set; }

            public IList<Gsm> gsm { get; set; }

            public IList<Sim> sims { get; set; }

            public Gps gps { get; set; }

            /// <value>Array of events</value>
            public IList<IList<dynamic>> events { get; set; }

            /// <value>Array of sensors</value>
            public IList<Object[]> sensors { get; set; }

            /// <value>
            /// These are inputs on the device that are connected to digital sensors that can only be
            /// in a high/on/1 or low/off/0 status. The inputs are defined as a string where each input
            /// status is defined as a single digit "1" or "0" from left to right, i.e., in the
            /// string '11000000' inputs 1 and 2 is high/on/1 and input 8 is low/off/0
            /// </value>
            public string inputs { get; set; }

            /// <value>
            /// Outputs are typically one or more relays on the device that can be toggled on and off
            /// by the device itself, which in turn can control an external device such as an LED light,
            /// a pump motor, etc. The outputs are defined as a string where each output status is
            /// defined as a single digit "1" or "0" from left to right, i.e., in the string '110'
            /// outputs 1 and 2 is high/on/1 and output 3 is low/off/0.
            /// </value>
            public string outputs { get; set; }

            /// <value>
            /// Similar to standard inputs on the device, the auxiliary inputs determines the status
            /// of inputs. However, in this case the inputs are usually provided by an external
            /// I/O device connected to the tracking device. The auxiliary inputs field is also an
            /// array of input strings instead of a single string. The reason for this is that we need
            /// to support "banks" of inputs, i.e. multiple such external devices providing input
            /// statuses.
            ///
            /// A typical example might be two banks of 8 inputs each, i.e.: ['11000000', '00010100']
            ///
            /// Each string in the array works the same as a single input string, see Inputs
            /// Field (inputs) details above.
            /// </value>
            public IList<string> aux_inputs { get; set; }

            /// <value>
            /// Analog inputs are mostly used when devices have the capability to record sensors that
            /// can provide a range of values, such as temperature or voltages. The field consists of
            /// an array of floating point numbers.
            /// </value>
            public IList<float> an_inputs { get; set; }

            /// <value>An array of OBD II - Mode 01 PID's and their values</value>
            public ObdII obd_ii { get; set; }

            /// <value>
            /// An array of arrays that contains CAN bus data. Each entry of CAN bus data
            /// consists of a key-value pair where the first element in the array is the CAN bus
            /// ID and the second element is the CAN bus value.
            /// </value>
            public IList<IList<int>> can_bus { get; set; }
        }

        public class Device
        {
            /// <value>Either "imei" or "code"</value>
            public string identifier { get; set; }

            /// <value>The device's IMEI (if identifier field is "imei")</value>
            public string imei { get; set; }

            /// <value>The device's serial no or identification code (if identifier field is "code")</value>
            public string serial_no { get; set; }

            /// <value>The device's firmware version (e.g. "1.04")</value>
            public string firm_ver { get; set; }

            /// <value>The device type, e.g. "teltonika"</value>
            public string type { get; set; }

            /// <value>The device model, only available on some units</value>
            public string model { get; set; }
        }

        public class Network
        {
            /// <value>The device IP v4 address</value>
            public string remote_ipv4 { get; set; }

            /// <value>The device IP v6 address</value>
            public string remote_ipv6 { get; set; }

            /// <value>The remote port of the connecting device.</value>
            public int? remote_port { get; set; }

            /// <value>The device MAC address</value>
            public string mac { get; set; }
        }

        public class Gsm
        {
            /// <value>Cell ID'd of network base transceiver stations, ranges 0-65,535 on GSM/CDMA and 268,435,455 on UMTS and LTE networks</value>
            public IList<int> cid { get; set; }

            /// <value>UTRAN Cell ID's, ranges 0-65,535 on GSM/CDMA and 268,435,455 on UMTS and LTE networks</value>
            public IList<int> lcid { get; set; }

            /// <value>Location area codes</value>
            public IList<int> lac { get; set; }

            /// <value>Carrier codes</value>
            public int? carrier { get; set; }

            /// <value>Received signal strength indications (in dBm)</value>
            public IList<int> rssi { get; set; }

            /// <value>Mobile country codes</value>
            public IList<string> mcc { get; set; }

            /// <value>Mobile network codes</value>
            public IList<string> mnc { get; set; }

            /// <value>Received channel power indicators (in dBm)</value>
            public IList<int> rcpi { get; set; }

            /// <value>Signal strength raw value, unit dependant and is often a number ranging from 1 to 5.</value>
            public int? ss_value { get; set; }

            /// <value>Signal strength percentage, calculated from raw signal strength value</value>
            public int? signal_str { get; set; }

            /// <value>Determines data/roaming mode, available values are:
            /// "home_stop", "home_move", "roam_stop", "roam_move", "unknown_stop", "unknown_move"
            /// </value>
            public string data_mode { get; set; }

            /// <value>Gsm Status Flags, can contain any combination of the following:
            /// "engine", "network", "data", "connected", "voice_call", "roaming"
            /// </value>
            public IList<string> status { get; set; }
        }

        public class Sim
        {
            /// <value>
            /// Mobile Station International Subscriber Directory Number. A number uniquely
            /// identifying a subscription in a GSM or a UMTS mobile network. Simply put, it is the
            /// mapping of the telephone number to the SIM card in a mobile/cellular phone.
            /// </value>
            public string msisdn { get; set; }

            /// <value>
            /// Integrated Circuit Card Identifier. ICCIDs are stored in the SIM cards and are
            /// also printed on the SIM card during a personalisation process.
            /// </value>
            public string iccid { get; set; }

            /// <value>
            /// International Mobile Subscriber Identity: Used to identify the user of a cellular
            /// network and is a unique identification associated with all cellular networks. It
            /// is stored as a 64 bit field and is sent by the phone to the network.
            /// </value>
            public string imsi { get; set; }
        }

        public class Gps
        {
            /// <value>The ISO 8601 GPS timestamp when the position was captured.</value>
            public DateTime timestamp { get; set; }

            /// <value>Latitude in decimal degrees (-90.0 to +90.0)</value>
            public float latitude { get; set; }

            /// <value>Longitude in decimal degrees (-180.0 to +180.0)</value>
            public float longitude { get; set; }

            /// <value>Altitude in meters</value>
            public float altitude { get; set; }

            /// <value>Speed in kmh</value>
            public float speed { get; set; }

            /// <value>Compass heading in degrees (0.0 to 360.0)</value>
            public float heading { get; set; }

            /// <value>Number of satellites used</value>
            public int satellites { get; set; }

            /// <value>Current activity: parked, idling, driving, walking, running, unknown</value>
            public string activity { get; set; }

            /// <value>Current GPS odometer</value>
            public int? odometer { get; set; }

            /// <value>Current trip GPS odometer</value>
            public int? trip_odo { get; set; }

            /// <value>GNSS enabled (Global Navigation Satellite System enabled, see Teltonika docs)</value>
            public bool? gnss { get; set; }

            /// <value>Horizontal dilution of precision</value>
            public float? hdop { get; set; }

            /// <value>Vertical dilution of precision</value>
            public float? vdop { get; set; }

            /// <value>Position (3D) dilution of precision</value>
            public float? pdop { get; set; }

            /// <value>Time dilution of precision</value>
            public float? tdop { get; set; }

            /// <value>An array of GPS fix status flags:
            /// "fixed", "predicted", "diff_corrected", "last_known", "invalid_fix", "2d", "logged", "invalid_time"
            /// </value>
            public IList<string> fix { get; set; }
        }

        public class ObdII
        {
            /// <value>An array of OBD II (Mode 01) PID's and their values</value>
            public IList<Object[]> mode_01 { get; set; }

        }
    }
}
