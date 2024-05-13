using System.Reflection;

namespace Shared.VehicleTracking;
public class VehicleTrackingRoot
{
	public VehicleTrackingRoot()
	{
		vehiclesTrackingNotification = new List<VehicleTrackingNotification>();
	}
	public List<VehicleTrackingNotification> vehiclesTrackingNotification { get; init; }
}
public class VehicleTrackingNotification
{
	public Vehicle vehicle { get; init; }
	public Position position { get; init; }
}
public class Position
{
	public DateTime time { get; init; }
	// NOTE: Mx sends coordinates as STRINGS, but IVU expects float coordinates. Waiting on Mx to make the change.
	public List<float> coordinates { get; init; }
	public int heading { get; init; }
	public string source { get; init; }

	// RTSI sends coordinates as [latitude, longitude] and IVU expects coordinates in terms of [longitude, latitude]. 
	public void SwapCoordinates()
	{
		(coordinates[0], coordinates[1]) = (coordinates[1], coordinates[0]);
	}

}
public class Vehicle
{
	public string division { get; init; }
	public int? externalNumber { get; init; }
	public string GetPrimaryKey()
	{
		return division + externalNumber?.ToString() ?? "0";
	}

	public bool IsValid() => externalNumber != null;
}

