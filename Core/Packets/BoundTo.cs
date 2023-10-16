namespace Core.Packets;

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public class BoundTo : Attribute
{
    public PacketDestination Destination { get; set; }

    public BoundTo(PacketDestination destination)
    {
        Destination = destination;
    }
}
