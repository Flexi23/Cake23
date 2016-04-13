# Cake23
WebSocket Hub for Kinect v2 and MIDI controllers

This is a Visual Studio solution for a minimal HTTP Server based on OWIN to host a SignalR WebSocket hub to broadcast live skeletal tracking from the Kinect v2 sensor.

It depends on the Kinect SDK 2.0 from Microsoft that must be installed on your computer, that limits the usage to Windows 8 or 10 only, sorry. Then again, the clients run in all web browser that support WebGL and WebSockets.

The self-host mode requires elevated rights and to run it from the IDE you must start Visual Studio as Administrator. When you host the server yourself, don't forget to open the port in the Windows Firewall.

What you see is what you get. I've put chunks of 1 or 2 hours into it every other day over the last half year and this is a continuing work in progress. The application starts as a window. Leave the computer name, use localhost, or enter an address of a remote instance. The server hosts a handful of static resources and also integrates a Razor template controller to bind the js resources in the delivered web pages to the serving URLs. Try running a template file from [WebTemplates](Cake23/WebTemplates) like http://localhost:9000/clients/burningman
To add a new [Client](Cake23/Connection/Clients), just add a new Class and let it inherit from the abstract base class. There is a Factory that reflects it out for the GUI and WebSocket hub bindings, sets it up on application startup. At the moment, the application Connect button will start available clients. It's only the Kinect v2 and a Midi device wrapper now but it is designed to be easily extensible for anything that has a C# API. Even websites should be able to register as a client and be broadcasted back again to registered listeners with no big effort. Later, when there will be hopefully more client types, each one should be startable individually.

Shoot me with questions.