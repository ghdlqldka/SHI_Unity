using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class _LocationInfo
{
    //
    // Summary:
    //     Geographical device location latitude.
    public float latitude;
    //
    // Summary:
    //     Geographical device location latitude.
    public float longitude;
    //
    // Summary:
    //     Geographical device location altitude.
    public float altitude;
    //
    // Summary:
    //     Horizontal accuracy of the location.
    public float horizontalAccuracy;
    //
    // Summary:
    //     Vertical accuracy of the location.
    public float verticalAccuracy;
    //
    // Summary:
    //     Timestamp (in seconds since 1970) when location was last time updated.
    public double timestamp;

    public _LocationInfo(LocationInfo info)
    {
        altitude = info.altitude;
        latitude = info.latitude;
        longitude = info.longitude;
        horizontalAccuracy = info.horizontalAccuracy;
        verticalAccuracy = info.verticalAccuracy;
        timestamp = info.timestamp;
    }
}