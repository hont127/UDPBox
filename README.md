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
</BR>
### UDPBoxContainer.cs example:
qweqwe
</BR>
### Package format:
```C#
Package:  HEAD(bytes)|TYPE(short)|MAGICNUMBER(ushort)|ID(short)|ContentLength(uint)|Args(bytes)
and through the 'UDPClient' send and received.
```
### Process Handler example:
```C#
class ExampeHandler : HandlerBase
{
    ExamplePackage mTemplate;//Serialize or Deserialize.


    public ExampeHandler(byte[] packageHead)
    {
        mTemplate = new ExamplePackage(packageHead);//init template package.
    }

    protected override short[] GetCacheProcessableID()//Set processable id.
    {
        return new short[] { 1001, 1002 };
    }

    public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
    {
        if (!mTemplate.Deserialize(packageBytes)) return;//Deserialize.

        //Process...

        udpBox.SendMessage(mTemplate.Serialize(), ipEndPoint);//Sendback to ipEndPoint.
    }
}
```