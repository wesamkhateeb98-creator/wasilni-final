# Passenger Scenario — Wasilni Tracking System

## Overview

The passenger opens the app, sees active buses on their line, picks one, adds a location-based booking, and can cancel it. All real-time updates flow via SignalR.

---

## New Entity: `BookingEntity`

| Field         | Type            | Description                          |
|---------------|-----------------|--------------------------------------|
| `Id`          | `int`           | Primary key                          |
| `TripId`      | `int`           | FK → `TripEntity` (cascade delete)   |
| `PassengerId` | `int`           | FK → `AccountEntity` (restrict)      |
| `Latitude`    | `double`        | Passenger pick-up latitude           |
| `Longitude`   | `double`        | Passenger pick-up longitude          |
| `Status`      | `BookingStatus` | `Waiting` / `PickedUp` / `Cancelled` |
| `CreatedAt`   | `DateTime`      | UTC creation time                    |

```csharp
public enum BookingStatus { Waiting, PickedUp, Cancelled }
```

---

## REST API Endpoints

### 1. GET /api/v1.0/trips/active
Get all active buses. Optionally filter by line.

```
Authorization: Passenger | Admin
Query: ?lineId=1   (optional)
```

**Response:** `List<GetTripResponse>`
```json
[
  {
    "id": 3,
    "busId": 7,
    "busPlate": "WAS-1000",
    "lineId": 1,
    "lineName": "Main Line",
    "status": "Active",
    "latitude": 33.5154,
    "longitude": 36.2785,
    "anonymousCount": 2,
    "startedAt": "2026-03-26T08:00:00Z"
  }
]
```

> `latitude`/`longitude` come from **in-memory cache** (last GPS ping from driver), not the DB.

---

### 2. POST /api/v1.0/trips/{id}/bookings
Passenger adds a booking with their current location.

```
Authorization: Passenger
```

**Request body:**
```json
{
  "latitude":  33.5120,
  "longitude": 36.2750
}
```

**Validation:**

| Field       | Rule                     |
|-------------|--------------------------|
| `latitude`  | Between `-90` and `90`   |
| `longitude` | Between `-180` and `180` |

**Response:** `GetBookingResponse`
```json
{
  "id": 12,
  "tripId": 3,
  "passengerId": 45,
  "latitude": 33.5120,
  "longitude": 36.2750,
  "status": "Waiting",
  "createdAt": "2026-03-26T08:05:00Z"
}
```

**Business rules:**
- Trip must be **Active** → `404` if not found or ended
- Passenger can only have **one active booking per trip** → `409` if duplicate

**SignalR side effect:**
After saving, sends `OnBookingAdded` to group `trip-{tripId}` → driver sees the passenger pin on the map.

---

### 3. DELETE /api/v1.0/trips/{id}/bookings
Passenger cancels their active booking on a trip.

```
Authorization: Passenger
```

**Response:** `MutateResponse`
```json
{ "id": 12 }
```

**Business rules:**
- Trip must be **Active**
- Booking must exist and be in **Waiting** status

**SignalR side effect:**
Sends `OnBookingCancelled` to group `trip-{tripId}` → driver removes passenger pin from map.

---

## SignalR Events

### Passenger subscribes (client → server)

```javascript
// Follow a specific bus
await connection.invoke("SubscribeToTrip", tripId);

// Discover new buses appearing on a line
await connection.invoke("SubscribeToLine", lineId);

// Stop following a bus
await connection.invoke("UnsubscribeFromTrip", tripId);
```

### Passenger receives (server → client)

| Event                     | Group           | Payload                                        | When                  |
|---------------------------|-----------------|------------------------------------------------|-----------------------|
| `OnLocationUpdated`       | `trip-{id}`     | `{ tripId, latitude, longitude, updatedAt }`   | Driver sends GPS      |
| `OnTripStarted`           | `line-{id}`     | Full `GetTripResponse`                         | Driver starts trip    |
| `OnTripEnded`             | `trip-{id}`     | `{ tripId }`                                   | Driver ends trip      |
| `OnAnonymousCountUpdated` | `trip-{id}`     | `{ tripId, count }`                            | Driver adjusts count  |

### Driver receives (from REST → hub)

| Event               | Group       | Payload                       | When                    |
|---------------------|-------------|-------------------------------|-------------------------|
| `OnBookingAdded`    | `trip-{id}` | Full `GetBookingResponse`     | Passenger books         |
| `OnBookingCancelled`| `trip-{id}` | `{ bookingId }`               | Passenger cancels       |

---

## Full Flow Diagram

```
Passenger opens app
    │
    ├─► REST: GET /trips/active?lineId=1
    │         ← List of active buses with last known location
    │
    ├─► SignalR: SubscribeToLine(1)      ← see new buses starting
    ├─► SignalR: SubscribeToTrip(3)      ← follow bus #3 location
    │
    │   [Real-time location updates every ~3s]
    │   ← OnLocationUpdated: { tripId:3, lat:33.51, lng:36.28 }
    │
    ├─► REST: POST /trips/3/bookings  { lat:33.512, lng:36.275 }
    │         ← { id:12, status:"Waiting", ... }
    │         [Driver receives OnBookingAdded → sees pin on map]
    │
    ├─► REST: DELETE /trips/3/bookings   (change of plans)
    │         ← { id:12 }
    │         [Driver receives OnBookingCancelled → pin removed]
    │
    └─► SignalR: ← OnTripEnded: { tripId:3 }
                  [Bus removed from map]
```

---

## Architecture

| Layer          | File                            | Role                               |
|----------------|---------------------------------|------------------------------------|
| Domain         | `BookingEntity.cs`              | Entity with `Create()` factory     |
| Domain         | `BookingStatus.cs`              | Enum                               |
| Domain         | `GetBookingModel.cs`            | Read model (record)                |
| Application    | `IBookingRepository.cs`         | Repository contract                |
| Application    | `ITripService.cs`               | 3 new passenger methods            |
| Application    | `TripService.cs`                | Business logic                     |
| Application    | `TripEntityExtensions.cs`       | `BookingEntity.ToModel()`          |
| Infrastructure | `BookingRepository.cs`          | EF Core queries                    |
| Infrastructure | `BookingEntityConfiguration.cs` | Cascade delete config              |
| Presentation   | `TripsController.cs`            | 3 endpoints + `IHubContext` notify |
| Presentation   | `AddBookingRequest.cs`          | Request DTO                        |
| Presentation   | `GetBookingResponse.cs`         | Response DTO                       |
| Presentation   | `AddBookingRequestValidator.cs` | Lat/lng bounds validation          |
| Presentation   | `TrackingHubEvents.cs`          | `OnBookingAdded/Cancelled` methods |

---

## Error Reference

| Scenario                    | Exception              | Status |
|-----------------------------|------------------------|--------|
| Trip not found / ended      | `NotFoundException`    | 404    |
| Already booked on this trip | `AlreadyExistsException` | 409  |
| Booking not found           | `NotFoundException`    | 404    |
| Invalid lat/lng             | Validation error       | 400    |
