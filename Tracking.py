import socket, threading
import direct.directbase.DirectStart
from pandac.PandaModules import *
from direct.task import Task
from direct.showbase import *
import json, sys, re
from xml.etree.ElementTree import Element

verbose = False;
for arg in sys.argv[1:]:
    if arg == '-v':
        verbose = True;
        
def output(msg):
    if verbose:
        print msg; 

def format_coordinates(co):
    return ('[%.4f, %.4f, %.4f]' % (co[0], co[1], co[2]))
   
class Tracker_Obj:
    def __init__ (self, vrpnclient, id, pos):
        self.id = id 
        self.tracker_node = TrackerNode(vrpnclient, id)
        self.tracked_node = render.attachNewNode('tracked' + id)
        base.dataRoot.node().addChild(self.tracker_node)
        self.t2n = Transform2SG('t2n' + id)
        self.tracker_node.addChild(self.t2n)
        self.t2n.setNode(self.tracked_node.node())
        self.display = aspect2d.attachNewNode(TextNode('display'))
        self.display.setScale(0.1)
        self.display.setPos(pos[0],pos[1],pos[2])
        
class TrackerServer:
    def __init__ (self, up_val):
        
        if (up_val == 'Y_UP'):
            self.up = 'y';
        elif (up_val == 'Z_UP'):
            self.up = 'z';
        else:
            print ("Error: Specify a coordinate for 'up'");
            exit();
        
        self.INCOMING_PORT = 42068
        self.OUTGOING_PORT = 42069
        self.OUTGOING_IP = ""
        
        self.clients = []
        self.objects = {}
       
        self.vrpnclient = VrpnClient ('localhost')
       
        # Bind Socket
        self.COMM = socket.socket(socket.AF_INET, # Internet
                         socket.SOCK_DGRAM)  # UDP
       
        # Recieving
        self.COMM.bind ((self.OUTGOING_IP, self.INCOMING_PORT))
        
        # Listen
        self.listener = threading.Thread(target=self.listen)
        self.listener.start()
               
        # Task Manager
        taskMgr.add (self.update,'update')
        
        # Display
        camera.lookAt(0,0,0)
        
       
    def add_obj(self, id):
        obj = Tracker_Obj(self.vrpnclient, id, [-1,0, 0.8 - len(self.objects) * 0.35])
        self.objects[obj.id] = obj
        
    def listen(self):
        output('Begin listening...\n')
       
        while (True):
            try:  
                msg, addr = self.COMM.recvfrom(1024)
                output('Message from ' + str(addr) + ': ' + msg)
                if msg == 'ADD ME':
                    output(addr[0] + ' added')
                    self.clients.append(addr[0])
            except socket.error as error:
                if (error.errno == 10054):
                    pass
                else:
                    print error.errno
      
                    
    def update(self, t):
        self.vrpnclient.poll()
        
        if self.objects:

            for id, obj in self.objects.iteritems():  
                panda_pos = obj.tracked_node.getPos()
                
                if (self.up == 'y'):
                    position = [panda_pos.getX(), panda_pos.getZ(), panda_pos.getY()]
                else:
                    position = [panda_pos.getX(), panda_pos.getY(), panda_pos.getZ()]
            
                panda_hpr = obj.tracked_node.getHpr()
                hpr = [panda_hpr.getX(), panda_hpr.getY(), panda_hpr.getZ()]
                

                
                msg = {}
                msg['code'] = 1;
                msg['id'] = id;
                msg['pos'] = position;
                msg['hpr'] = hpr;
                msg = json.dumps(msg);
                
                display_text = obj.id + ' :\n';
                display_text += 'Pos   ' + format_coordinates(position) + '\n';
                display_text += 'Hpr   ' + format_coordinates(hpr) + '\n';
                
                obj.display.node().setText(display_text)
                
                for client in self.clients:
                    self.COMM.sendto(msg, (client, self.OUTGOING_PORT))
        
        # Ensure you have objects to transmit
        else:
            for client in self.clients:
                msg = {}
                msg['code'] = 0;
                self.COMM.sendto(msg, (client, self.OUTGOING_PORT))
                
        return Task.cont

server = TrackerServer('Y_UP')
server.add_obj('Visor')
server.add_obj('Wand')
server.add_obj('Staff')
run()
