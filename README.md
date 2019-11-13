# UDPBox
The simple LAN-Network unity3d game server

</BR>1.Support for message compression (Ionic.Zip)
</BR>2.Support ACK message confirmation (default is UDP regular message)
</BR>3.Send, receive, and logically process is 3 independent threads
</BR>4.Find the client through broadcast
</BR>
</BR>5.Example in: Extensions/GetMasterClientCount/... or Extensions/...
</BR>
</BR>
### UDPBox.cs example:
```C#
var udpClients = UDPBoxUtility.GeterateUdpClientsArray(1235, 1236);
mUDPBox = new UDPBox(udpClients, UDPBoxUtility.DefaultHead);
mUDPBox.RegistHandler(...);
mUDPBox.RegistHandler(...);
mUDPBox.RegistHandler(...);
mUDPBox.RegistHandler(...);
mUDPBox.RegistHandler(...);
mUDPBox.RegistHandler(...);
mUDPBox.Start();
```