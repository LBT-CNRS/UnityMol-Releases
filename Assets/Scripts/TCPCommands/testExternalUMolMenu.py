import struct
import socket
import tkinter as tk
from functools import partial
import zmq


class UMolCommand:

    def __init__(self):
        self.isConnected = False
        
    def connect(self, ip="localhost", port= 5555):
        self.context = zmq.Context()
        self.socket = self.context.socket(zmq.REQ)
        self.socket.connect("tcp://"+ip+":"+str(port))
        self.isConnected = True

    def disconnect(self, ):
        # if self.isConnected:
            # self.s.close()
        self.isConnected = False

    def send(self, com):
        if self.isConnected:
            encodedCom = com.encode()
            # self.s.send(encodedCom)
            print("encoded: ",encodedCom)
            self.socket.send(encodedCom)
            message = self.socket.recv().decode()
            return message

class Application(tk.Frame):
    def __init__(self, master=None):
        super().__init__(master)
        self.master = master
        self.pack()
        self.create_widgets()
        self.selections = []
        self.selectionButtons = []
        self.u = UMolCommand()
        self.u.connect()

    def create_widgets(self):
        self.refreshB = tk.Button(self)
        self.refreshB["text"] = "Refresh selections"
        self.refreshB["command"] = self.refreshSelections
        self.refreshB.pack(side="top")




    def refreshSelections(self):
        print("Refreshing...")
        
        sels = self.u.send("getSelectionListString()")
        self.selections = []
        if sels and len(sels) > 0:
            tmp = sels.replace("[","").replace("]","").split(", ")
            self.selections = [i for i in tmp if len(i.strip()) != 0]

        for i in self.selectionButtons:
            i.destroy()
        self.selectionButtons = []
        curid = 0
        for i in self.selections:
            butto = tk.Button(self)
            butto["text"] = i
            self.selectionButtons.append(butto)
            butto.pack(side="top")
            butto["command"] = partial(self.printMe, curid)
            curid+=1

    def printMe(self, b):
        selName = self.selections[b]
        shown = self.u.send("areRepresentationsOn('"+selName+"', 'hb')")
        print(shown)
        if len(shown) > 0:
            shown = shown == "True"
        else:
            shown = True
        if not shown:
            command = "showSelection('"+selName+"', 'hb')"
        else:
            command = "hideSelection('"+selName+"', 'hb')"

        res = self.u.send(command)

root = tk.Tk()
app = Application(master=root)
app.mainloop()
app.u.disconnect()
root.destroy()

