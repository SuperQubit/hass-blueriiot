# hass-blueriiot
Blueriiot Blue Connect Add-On for Home Assistant

This add-on will periodically poll the Blueriiot API to retrieve sensor data from your Blue Connect unit.

This add-on is in development/testing at present. If multiple pool is present the first one is selected.

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
