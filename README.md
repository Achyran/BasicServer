Hi Tobi here.
To be honest id don't know what to write here, so I am just gonna tell you the aim of this project.
I want to create a lib for a basic server setup. The idea is to have a starting for networking in games.
The idea is to have it as generic as possible. The Project structure resembles that. The BasicClient and BasicServer 
are the consumers in this case. code in there shows you how to use the other 3 projects. Packages is a shared project. In here we have
 the Abstract Package a PackageFactory and Package classes. I the future i want to create a generator, so that a package can be 
 written and during compiling a corresponding read and write function will be generated in the Factory. The last 2 projects are the 
 server and the Client Code. There only responsibility should be the tcp and Udp Connection. Right now i have some handling code in 
 there, this will be removed in the future. In the end the server and the client class will have Package Queues, so that the 
 BasicServer and BasicClient can handel the packages then. So that when you want to create a Game. You would import the Server project
  and Client project with the Dependency on the Package Project. Create your Packages and Send and receiving them through the Queues.

  What is working:
  Right now TCP is Implemented and A Message can be send with the message class. A heartbeat is also send, but it does nothing right now.

  What will be done:
    - move the queue handling to the example projects.
    - Implement UDP
    - Create settings for the server and client
    - Determine a healthy connection through the heartbeat
    - Handle disconnections.
    - Expose clients Ids on the server (to The BasicServer)
    - Disconnect Functions on Client and Server 


    ---------------------------------------------------------------------------------------------
    7.2.24

    Refactored the basic server and client. Now only packages tied to connection are handled by those classes, other packages are put
    in a to be handledQueue on the server and a recivedQueue on the client. Both queues are public and should be handled Outside of those classses.
    I have also added a IsConnected bool that is refreshed by the heartbeat. 