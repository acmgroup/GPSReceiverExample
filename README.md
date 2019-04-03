# C# GPS Receiver Example

Welcome to the GPS Receiver example project for C# and .NET. This 
is an example project which allows you to receive and decode GPS 
messages from our ACMGroup Gateway Services (GWS).

We use a popular opensource message queuing/brokering system called
[RabbitMQ](https://www.rabbitmq.com) for our own internal use and 
to push messages to 3'rd party clients. This project connects to
a RabbitMQ server and will consume GPS messages received from it.

# Setup Instructions

This project have been designed and tested in Microsoft Visual Studio 
Community 2017: v15.9.6 and the Microsoft .NET Framework: v4.7.03056 
(projected targeted at 4.6.1)

## Cloning the Repository

You will require a git client to clone the project to your development
environment. On Linux/Mac you can use the standard git client provided with
your distribution or on Windows you can use a console based client such as 
[get-scm](https://git-scm.com) or the 
[Github extension for Visual Studio](https://visualstudio.github.com).

Clone the project into a directory of your choice using:

`git clone https://github.com/acmgroup/GPSReceiverExample.git`

or when using the Visual Studion Github plugin go to the `Team Explorer`
Tab and click on `Clone` under the Github connection and then paste
https://github.com/acmgroup/GPSReceiverExample.git into the address bar.

Once you have cloned and opened the project in Visual Studion you can 
start installing the required NuGet packages, see the section below.

## Installing NuGet Packages

The project requires two NuGet packages to be installed:
https://www.nuget.org/packages/RabbitMQ.Client (v12.0.1)
https://www.nuget.org/packages/Newtonsoft.Json (v5.1.0)

To install them, navigate to `Project -> Manage NuGet Packages...` and
select the `Browse` tab. Then search for `RabbitMQ.Client` and click on
the `[Install]` button. Do the same for `Newtonsoft.Json`.

Alternatively you can navigate to 
`Tools -> NuGet Package Manager -> Package Manager Console` and enter
the following two commands:

```
Install-Package RabbitMQ.Client -ProjectName GPSReceiverExample
Install-Package Newtonsoft.Json -ProjectName GPSReceiverExample
```

Once the packages have been successfully installed you can start configuring
the application, see the section below:

## Project Settings

If you have been in contact with us, we would have supplied you with the credentials
to configure the project in order to connect to our services and to consume
messages from it.

To fill in these settings, navigate to: 
`Project -> GPSReceiverExample Properties...` and select the `Settings` tab.

Change `ServerHostname`, `ServerUsername`, `ServerPassword` and any other
properties we may have supplied you with.

Once done, press `Ctrl+S` to save the settings.

## Running the Project

The project is a console based application and hence does not have a GUI. When you 
run the project (by clicking on Start in the toolbar) it should connect to the
server and setup everything you need in order to start receiving messages. Look at
the output of the console to see if there are any errors.

The project will wait for incomming JSON based GPS messages, when received the GPS
message will be decoded into various classes, the results will be printed and an
Acknowledgement will be sent back to the server.

See the details below for the type of messages you may receive. In this case all
other message types are ignored except for `gps` message types.

# Universal GPS JSON Message Data Structure v1

The ACMGroup Universal JSON Message Data Structure v1 is a standard structure we
have designed to transmit telemetry data such as GPS data between our telemetry 
gateway services (GWS) and our processing services or to 3'rd party services.

**Message Types:**

1. register: Unit login/registration
2. heartbeat: Same as keep-alive type message
3. **gps: Contains new GPS related data**

## Message Type: register

Some units performs a "login" or "registration" when it first starts 
communicating with the server. This is typically done by TCP units
which maintains a TCP connection and it will usually transmit its
identification (e.g. IMEI) along with the message.

```json
{
    "message_ver": 1,
    "message_type": "register",
    "gateway": "gateway.someserver.net",
    "port": 5027,
    "transmission": "tcp",
    "timestamp": "2018-02-16T13:51:32+00:00",
    "source": "device",
    "seq_no": 15,
    "valid": true,
    "device": {
        "identifier": "imei",
        "imei": "359568052512345",
        "serial_no": null,
        "firm_ver": null,
        "hardw_ver": null,
        "type": "teltonika",
        "model": null
    },
    "network": {
        "remote_ipv4": "192.168.0.5",
        "remote_ipv6": null,
        "remote_port": 54432,
        "mac": null
    }        
}
```

## Message Type: heartbeat

Some units transmits a "heartbeat" packet at regular intervals
to keep the TCP connection alive.

```json
{
    "message_ver": 1,
    "message_type": "heartbeat",
    "timestamp": "2018-02-16T13:51:32+00:00",
    "imei": "359568052512345",
    "serial_no": null
}
```

## Message Type: gps

A GPS message mostly contains GPS data transmitted from a device
but also a whole host of other data relevant to the position and the
device's status.

```json
{
    "message_ver": 1,
    "message_type": "gps",
    "gateway": "gateway.someserver.net",
    "port": 5027,
    "transmission": "tcp",
    "timestamp": "2018-02-16T13:51:32+00:00",
    "source": "device",
    "seq_no": 15,
    "valid": true,
    "device": {
        "identifier": "imei",
        "imei": "359568052512345",
        "serial_no": null,
        "firm_ver": null,
        "type": "teltonika",
        "model": null
    },
    "network": {
        "remote_ipv4": "192.168.0.5",
        "remote_ipv6": null,
        "remote_port": 54432,
        "mac": null
    },
    "gsm": [
        {
            "cid": [],
            "lcid": [],
            "lac": [2],
            "carrier": null,
            "mcc": [],
            "mnc": [],
            "rssi": [],
            "rcpi": [],
            "signal_lvl": 4,
            "signal_str": 80,
            "data_mode": null,
            "status": [
                "engine",
                "network",
                "data",
                "connected",
                "voice_call",
                "roaming"
            ]
        }
    ],
    "sims": [
        {
            "msisdn": null,
            "iccid": null,
            "imsi": null
        }
    ],
    "gps": {
        "timestamp": "2018-02-16T13:51:30+00:00",
        "latitude": -25.711470,
        "longitude": 28.152464,
        "altitude": 1047.0,
        "speed": 17.5,
        "heading": 112.0,
        "satellites": 8,
        "activity": "driving",
        "odometer": 0,
        "trip_odo": null,
        "gnss": true,
        "hdop": 2.0,
        "vdop": 2.0,
        "pdop": 2.0,
        "tdop": null,
        "fix": [
            "fixed",
            "predicted",
            "diff_corrected",
            "last_known",
            "invalid_fix",
            "2d",
            "logged",
            "invalid_time"
        ]
    },
    "events": [
        ["ZONE:1:ENTER"],
        ["GEOFENCE:ENTER"],
        ["HARSH_DRIVING:BRAKING", [
            ["X", -0.4531],
            ["Y", 0.00312]
        ]]
    ],
    "sensors": [
        ["ignition", "acc"],
        ["charging", true],
        ["accel_x", 0.23],
        ["accel_y", 0.0],
        ["accel_z", 0.0],
        ["ibat_volts", 4.59],
        ["ibat_ohms", null],
        ["ebat_volts", 0.25],
        ["driver_tag", "AA34553"],
        ["ibutton", "0A34C4D4AB33A2B3"],
        ["oil_temp", 55.75],
        ["oil_press", 0.9],
        ["fuel_level", 23.7]
    ],
    "inputs": "1000000",
    "outputs": "001",
    "aux_inputs": ["000000000"],
    "an_inputs": [0.56, 7.24],
    "obd_ii": {
        "mode_01": [
            [12, 2700],
            [13, 18]
        ]
    },
    "can_bus": [
        [81, 120],
        [82, 50],
        [83, 542234]
    ]
}
```

# Field Descriptions

## Primary Fields

* message_ver (int): The JSON message structure version
* message_type (string): register, gps, history, status, event, heartbeat
* gateway (string): The domain which received the message from the unit
* port (int): The server TCP/UDP the unit connected to.
* transmission (string): Transmission type: tcp, udp, http, https, sms
* timestamp (IS8601 string): The timestamp when the message was received at the gateway
* source: The message source, e.g. device, 3rd_party_service
* seq_no: Sequence number, usually provided by the unit
* valid: Whether the Gateway validated this as a "valid" message, i.e.
  anyone reading the data can use it as a valid position. It is recommended
  that if valid is false, that the message is either discarded, or
  stored but not used in any calculations/processing.
* activity: Can be any of the following: unknown, still, walking,
  running, driving, parked, idling

## Device Fields (device)

* primary_id (string): Either "imei" or "code"
* imei (string|null): The device's IMEI
* serial_no (string|null): The device's serial_no
* firm_ver (string|null): The device's firmware version (e.g. "1.04")
* hardw_ver (string|null): The device's hardware version (e.g. "1.04")
* type (string): The device type, e.g. "teltonika"
* model (string|null): The device model, e.g. "fmb_920" (not always available)

## Network Fields (network)

* remote_ipv4 (string|null): The device IP v4 address
* remote_ipv6 (string|null): The device IP v6 address
* remote_port (int): The remote port of the connecting device.
* mac (string|null): The device MAC address

## GSM Fields (gsm)

The GSM field is an array of objects with the following
fields:

* cid (array of int's): Cell ID's of network base transceiver station
* lcid (array of int's): UTRAN Cell ID
* lac (array of int's): Location area codes
* carrier (int|null): Carrier code
* rssi (array of int's): Received signal strength indication (in dBm)
* mcc (array of strings): Mobile country codes
* mnc (array of strings): Mobile network codes
* rcpi (array of int's): Received channel power indicators
* registration (int|null): Registration status: 0 - 5
* signal_lvl (int|null): Signal strength raw value, unit dependant and is often
  a number ranging from 0 to 5.
* signal_str (int|null): Signal strength percentage
* data_mode (string|null): Determines data/roaming mode, available values are
  * home_stop: Home on Stop
  * home_move: Home on Move
  * roam_stop: Roaming on Stop
  * roam_move: Roaming on Move
  * unknown_stop: Unknown on Stop
  * unknown_move: Unknown on Move
* status (array): Contains any of the following combination of strings depending
  on what the device provides:
  * "engine": GSM engine is available.
  * "network": Network is available.
  * "data": Data is available.
  * "connected": GSM data is connected
  * "voice_call": Voice call is currently active
  * "roaming": Roaming is currently active

## Sim Fields (sims)

The SIM field is an array of objects with the following
fields:

* msisdn (string|null): Mobile Station International Subscriber Directory
  Number. A number uniquely identifying a subscription in a GSM or a
  UMTS mobile network. Simply put, it is the mapping of the
  telephone number to the SIM card in a mobile/cellular phone.
* iccid (string|null): Integrated Circuit Card Identifier. ICCIDs are stored
  in the SIM cards and are also printed on the SIM card during a
  personalisation process.
* imsi (string|null): International Mobile Subscriber Identity: Used to
  identify the user of a cellular network and is a unique
  identification associated with all cellular networks. It is stored
  as a 64 bit field and is sent by the phone to the network.

## GPS Fields (gps)

* timestamp (IS8601 string|null): The GPS timestamp when the position was captured.
* latitude (float): Latitude in decimal degrees (-90.0 to +90.0)
* longitude (float): Longitude in decimal degrees (-180.0 to +180.0)
* altitude (float): Altitude in meters
* speed (float): Speed in km/h
* heading (float): Compass heading in degrees (0.0 to 360.0)
* satellites (int): Number of satellites used
* activity (string): Current activity: parked, idling, driving, walking, running, unknown
* odometer (int|null): Current GPS odometer
* trip_odo (int|null): Current trip GPS odometer
* gnss (bool|null): GNSS enabled (Global Navigation Satellite System enabled, see Teltonika docs)
* hdop (float|null): Horizontal dilution of precision
* vdop (float|null): Vertical dilution of precision
* pdop (float|null): Position (3D) dilution of precision
* tdop (float|null): Time dilution of precision
* fix (array): An array of GPS fix status strings.
  Here are the available values and their meanings:
  * "fixed": GPS coordinate fixed (valid)
  * "predicted": When the position update has a horizontal accuracy
    estimate that is less than the Horizontal Accuracy Threshold.
  * "diff_corrected": Differential correct, when the WAAS DGPS is enabled.
  * "last_known": Current GPS fix invalid but previous fix available and
    was used.
  * "2d": Set when less than 4 satellites are seen, typically means less
    accuracy and altitude not available.
  * "logged": Set when the position was logged and not directly forwarded
  * "invalid_time": Set when a valid time was not yet synced, typically
    after a power-up or reset.

Please note that the timestamp field can be null, which means that a
position was captured but the unit have not been able to time sync yet.

## Sensors

An array of arrays that contains sensor names and their values. Note that
no sensors are guaranteed to be submitted and therefore receiving
software should not rely on individual sensors being found consistently
or at all in the array of sensors.

Known sensors are:

* ignition (string): Possible values: "off", "acc", "on"
* charging (bool): Is the internal battery charging.
* accel_x, accel_y, accel_z (float): Accelerometer values
* ibat_volts (float): Internal battery voltage
* ebat_volts (float): External battery voltage
* driver_tag (string): Driver identification tag
* oil_temp (float): Engine oil temperature
* oil_press (float): Engine oil pressure
* fuel_level (float): Fuel level

## CAN Bus

An array of arrays that contains 
[CAN bus](https://en.wikipedia.org/wiki/CAN_bus) data. Each entry of CAN bus data 
consists of a key-value pair where the first element in the array is the CAN bus
ID and the second element is the CAN bus value. For example, the following array
contains one CAN bus entry with ID 81 and value of 120.

```
"can_bus": [
    [81, 120]
]
```

The next example have 3 CAN bus data entries with ID's 81, 82 and 83 respectively
and values of 120, 50 and 542234 respectively:

```
"can_bus": [
    [81, 120],
    [82, 50],
    [83, 542234]
]
```

## Events Field (events)

Each event consists of an array where the first element is a string
and the second element is an optional array of keys and values.

For example, a simple event without any additional data will simply
consist of this:

```["ZONE:1:ENTER"]```

whereas the following event also consists of an X and Y value which
is the accelerometer values of the X and Y axis:

```
["HARSH_DRIVING:BRAKING", [
    ["x", -0.4531],
    ["y", 0.00312]
]]
```

Other examples are:

```
["ZONE:1:ENTER"], ["ZONE:1:EXIT"]
["ZONE:2:ENTER"], ["ZONE:2:EXIT"]
["ZONE:3:ENTER"], ["ZONE:3:EXIT"]
["ZONE:4:ENTER"], ["ZONE:4:EXIT"]
["ZONE:5:ENTER"], ["ZONE:5:EXIT"]

["GEOFENCE:ENTER"], ["GEOFENCE:EXIT"]
["TRIP:START"], ["TRIP:END"]
["SPEEDING", [["value", 125]]]
["SPEEDING:START", [["value", 125]]]
["SPEEDING:END", [["value", 119]]]
["IDLE:START"], ["IDLE:END"]
["TOWING"]
["HARSH_DRIVING:BRAKING", [
    ["x", -0.4531],
    ["y", 0.00312]
]]
["HARSH_DRIVING:ACCELERATION", [
    ["x", 0.3531],
    ["y", 0.00286]
]]
["HARSH_DRIVING:BRAKING"], ["HARSH_DRIVING:ACCELERATION"]
["HARSH_DRIVING:TURNING"], ["HARSH_DRIVING:LEFT_TURN"], ["HARSH_DRIVING:RIGHT_TURN"]
["GREEN_DRIVING:ACCELERATION"], ["GREEN_DRIVING:BRAKING"], ["GREEN_DRIVING:CORNERING"]
["IMPACT:FRONT"], ["IMPACT:REAR"], ["IMPACT:LEFT"], ["IMPACT:RIGHT"], ["IMPACT:ROLLOVER"]
["JAMMING:STARTED"], ["JAMMING:ENDED"]
["UNIT:UNPLUGGED"]
```

## Inputs Field (inputs)

These are inputs on the device that are connected to digital sensors
that can only be in a high/on/1 or low/off/0 status. The inputs are
defined as a string where each input status is defined as a single
digit "1" or "0" from left to right, i.e., in the string '11000000'
inputs 1 and 2 is high/on/1 and input 8 is low/off/0.
```
Input No: 12345678
String  : 11000000
```

Inputs can be either null (no inputs) or a string of up to 16
characters.

## Outputs Field (outputs)

Outputs are typically one or more relays on the device that can be
toggled on and off by the device itself, which in turn can control an
external device such as an LED light, a pump motor, etc.

The outputs are defined as a string where each output status is defined
as a single digit "1" or "0" from left to right, i.e., in the string
'110' outputs 1 and 2 is high/on/1 and output 3 is low/off/0.

```
Output No: 123
String   : 110
```

Outputs can be either null (no outputs) or a string of up to 16
characters.

## Auxiliary Field (aux_inputs)

Similar to standard inputs on the device, the auxiliary inputs
determines the status of inputs. However, in this case the inputs are
usually provided by an external I/O device connected to the tracking
device. The auxiliary inputs field is also an array of input strings
instead of a single string. The reason for this is that we need to
support "banks" of inputs, i.e. multiple such external devices providing
input statuses.

A typical example might be two banks of 8 inputs each, i.e.:
['11000000', '00010100']

Each string in the array works the same as a single input string, see
**Inputs Field (inputs)** details above.

## Analog Inputs Field (an_inputs)

Analog inputs are mostly used when devices have the capability to record
sensors that can provide a range of values, such as temperature or
voltages.

The field consists of an array of floating point numbers.

## OBD II Field: (obd_ii)

May contain OBD II data, usually for mode 01 which will be an array of
PID's and their values or it will be null if OBD II is not supported or
supplied by the device.

Please see the [OBD-II](https://en.wikipedia.org/wiki/On-board_diagnostics#OBD-II)
and [OBD-II PID](https://en.wikipedia.org/wiki/OBD-II_PIDs)
specifications for more details.
