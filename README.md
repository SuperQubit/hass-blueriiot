# hass-blueriiot
Blueriiot Blue Connect Add-On for Home Assistant

This add-on will periodically poll the Blueriiot API to retrieve sensor data from your Blue Connect unit.

This add-on is in development/testing at present. If multiple pools are present you can choose which one to poll using the `PoolIndex` option.

Home Assistant AddOn Repository: https://github.com/SuperQubit/HASSAddons.

Original From: https://github.com/MikeJMcGuire/HASSAddons.

## Configuration
### BlueriiotUser: string
Set this field to your Blueriiot user name.

### BlueriiotPassword: string
Set this field to your Blueriiot password.

### MQTTBroker: string
Set this field to core-mosquitto to use the HA Mosquitto MQTT add-on. Otherwise, specify a host or host:port for an alternative MQTT server.

### MQTTTLS: true/false
Setting this option to true will force the MQTT client to attempt a TLS connection to the MQTT broker.

### PoolIndex: string
Comma separated list of pool indexes to poll. Example `"0,1"` will publish
measurements for the first two pools on your account.

### DeviceId: string
Unique identifier prefix for the MQTT devices. If not set, the first pool index
is appended to `hass-blueriiot` when a single pool is monitored.
Example:

```
"DeviceId": "my-pool"
```
