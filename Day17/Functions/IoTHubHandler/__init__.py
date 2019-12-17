import logging
import azure.functions as func
from itertools import groupby
import json

def main(
    event: func.EventHubEvent,
    msg: func.Out[str]):

    body = event.get_body().decode('utf-8')
    logging.info('Python EventHub trigger processed an event: %s', body)
    content = json.loads(body)
    events = []
    for x in content:
        events.append(DeviceEvent(**x))
    
    grouped = groupby(events, lambda x: x.deviceId )    
    for key, group in grouped:
        if(any(e for e in group  if e.temperature > 31.0)):
            sendObj = {
                "risedAlarm" : True,
                "devicidId" : key
            }
            msg.set(json.dumps(sendObj))
        else:
            logging.info("Any event from %s doesn't have temperature more then 31" % key)
    
class DeviceEvent:
    def __init__(self, messageId, deviceId, temperature, humidity):
        self.messageId = messageId
        self.deviceId = deviceId
        self.temperature = temperature
        self.humidity = humidity
    
    @classmethod
    def from_json(cls, json_string):
        json_dict = json.loads(json_string)
        return cls(**json_dict)
    
    def __repr__(self):
        return f'<DeviceEvent {self.messageId}'
    