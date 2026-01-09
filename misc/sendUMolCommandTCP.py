import struct
import socket

class UMolCommand:

    def __init__(self):
        self.isConnected = False
        self.separator = "$*$*"
        
    def connect(self, ip="localhost", port= 1423):
        self.s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.s.connect((ip, port))
        self.isConnected = True

    def disconnect(self, ):
        if self.isConnected:
            self.s.close()
        self.isConnected = False

    def send(self, com):
        if self.isConnected:
            com += self.separator
            encodedCom = com.encode()
            self.s.send(encodedCom)
            # data = self.s.recv(1024)
            # return data

    def receive(self):
        if self.isConnected:
            data = self.s.recv(1024).decode()
            data = [x for x in data.split(self.separator) if x]
            return data

def example():
    u = UMolCommand()
    u.connect()
    u.send('fetch("1KX2")')
    u.send('showSelection(last().ToSelectionName(), "s", SurfMethod.MSMS)')
    u.send('for i in last().currentModel.allAtoms:\n\tprint(i)')

u = UMolCommand()
u.connect()
u.send('fetch("1kx2")')
u.send("getSelectionListString()")
