namespace SoftPro.Wasilni.Domain.Enums;

public enum Role
{
    Passenger = 0,
    Admin = 1
}

public enum Permission
{
    None = 0,
    BusDriving = 1,
    ControlBus = 2,
    DrivingAndControlBus = 4,
    CoordinationStop = 5
}
