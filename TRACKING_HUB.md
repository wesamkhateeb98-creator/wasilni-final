# TrackingHub & TripService — توثيق تفصيلي

---

## 1. نظرة عامة على المعمارية

```
Client (Driver App)
       │
       │  SignalR (WebSocket)
       ▼
┌─────────────────┐
│  TrackingHub    │  ← Presentation Layer
│  (SignalR Hub)  │
└────────┬────────┘
         │ calls
         ▼
┌─────────────────┐
│  ITripService   │  ← Application Layer (interface)
│  TripService    │  ← Application Layer (implementation)
└────────┬────────┘
         │ uses
         ├──────────────────────┐
         ▼                      ▼
┌─────────────────┐    ┌─────────────────┐
│  IUnitOfWork    │    │  IMemoryCache   │
│  TripRepository │    │  (Location +    │
│  BusRepository  │    │   DriverTrip)   │
└────────┬────────┘    └─────────────────┘
         │
         ▼
┌─────────────────┐
│   AppDbContext  │  ← Infrastructure Layer
│   SQL Database  │
└─────────────────┘
```

---

## 2. Domain Layer — TripEntity

**الملف:** `SoftPro.Wasilni.Domain/Entities/TripEntity.cs`

### الخصائص

| Property | Type | Description |
|---|---|---|
| `BusId` | `int` | الباص المرتبط بهذه الرحلة |
| `DriverId` | `int` | السائق المكلف |
| `LineId` | `int` | الخط الذي يسير عليه الباص |
| `Status` | `TripStatus` | حالة الرحلة: `Active` أو `Ended` |
| `StartedAt` | `DateTime` | وقت بدء الرحلة (UTC) |
| `EndedAt` | `DateTime?` | وقت انتهاء الرحلة (null إذا لا تزال نشطة) |
| `AnonymousCount` | `int` | عدد الركاب الصاعدين بدون موقع |
| `Bus` | `BusEntity` | Navigation property للباص |

### الـ Constructor
```csharp
private TripEntity() { }  // لـ EF Core فقط
```
المُنشئ خاص — لا يمكن إنشاء `TripEntity` مباشرة من خارج الكلاس.

### Factory Method — Create
```csharp
public static TripEntity Create(int busId, int driverId, int lineId)
```
ينشئ رحلة جديدة بـ `Status = Active` و `StartedAt = DateTime.UtcNow`.

### الـ Methods

```csharp
public void End()
// يغير Status → Ended
// يسجل EndedAt = DateTime.UtcNow

public void AdjustAnonymous(int delta)
// يزيد أو ينقص AnonymousCount
// Math.Max(0, ...) → لا يصير سالب أبداً
```

---

## 3. Repository Layer

### ITripRepository

**الملف:** `SoftPro.Wasilni.Application/Abstracts/Repositories/ITripRepository.cs`

```csharp
Task<TripEntity?> GetActiveTripByDriverIdAsync(int driverId, CancellationToken ct);
// يجيب الرحلة النشطة للسائق
// يعمل Include للـ Bus + LineEntity (مطلوب لإرجاع اسم الخط)

Task<TripEntity?> GetActiveTripByBusIdAsync(int busId, CancellationToken ct);
// يتحقق هل الباص عنده رحلة نشطة حالياً
// يُستخدم في StartTrip لمنع تشغيل باص مفعّل مسبقاً

Task<TripEntity?> GetActiveByIdAsync(int id, CancellationToken ct);
// يجيب رحلة نشطة بالـ ID
// يُستخدم في EndTrip, UpdateLocation, AdjustAnonymous للتحقق
```

### TripRepository

**الملف:** `SoftPro.Wasilni.Infrastructure/Repositories/TripRepository.cs`

```csharp
// GetActiveTripByDriverIdAsync — يعمل Include لأنو التقرير يحتاج Bus.Plate + LineEntity.Name
.Include(t => t.Bus).ThenInclude(b => b.LineEntity)
.FirstOrDefaultAsync(t => t.DriverId == driverId && t.Status == TripStatus.Active)

// GetActiveTripByBusIdAsync — بدون Include لأنو فقط نتحقق من الوجود
.FirstOrDefaultAsync(t => t.BusId == busId && t.Status == TripStatus.Active)

// GetActiveByIdAsync — بدون Include لأنو فقط نتحقق ونعدّل
.FirstOrDefaultAsync(t => t.Id == id && t.Status == TripStatus.Active)
```

---

## 4. Application Layer — TripService

**الملف:** `SoftPro.Wasilni.Application/Services/TripService.cs`

### الـ Cache Keys

```csharp
LocationKey(int tripId)    → "bus-location:{tripId}"
// يخزن: BusLocationModel { Latitude, Longitude, UpdatedAt }
// الهدف: تحديث الموقع بدون كتابة على DB كل 3 ثوانٍ

DriverTripKey(int driverId) → "driver-trip:{driverId}"
// يخزن: int (tripId)
// الهدف: التحقق السريع في UpdateLocation بدون DB
```

---

### StartTripAsync

```csharp
Task<GetTripModel> StartTripAsync(int busId, int driverId, CancellationToken ct)
```

**الخطوات:**

```
1. اجلب الباص من DB (مع الـ Line)
   → NotFoundException إذا ما موجود

2. تحقق: هل driverId == bus.DriverId ؟
   → UnauthorizedException إذا لا

3. تحقق: هل يوجد رحلة نشطة لهذا الباص ؟
   ├── نعم + نفس السائق → RECONNECT
   │     → أعد تعيين cache
   │     → أرجع الرحلة الموجودة (ما تنشئ جديدة)
   └── نعم + سائق مختلف → AlreadyExistsException

4. أنشئ TripEntity.Create(busId, driverId, bus.LineId)
5. احفظ في DB
6. سجل في cache: DriverTripKey(driverId) = trip.Id
7. أرجع GetTripModel
```

**سيناريو الـ Reconnect:**
> إذا انقطع الإنترنت عن السائق وأعاد الاتصال وضغط StartTrip مرة ثانية،
> الـ service يتعرف إنو هو نفس السائق ويرجع الرحلة الموجودة بدل ما يرمي error.

---

### EndTripAsync

```csharp
Task EndTripAsync(int tripId, int driverId, CancellationToken ct)
```

**الخطوات:**

```
1. GetOwnedActiveTripAsync(tripId, driverId)
   → NotFoundException إذا ما موجودة الرحلة
   → UnauthorizedException إذا الرحلة لمو سائقها

2. trip.End() → Status = Ended, EndedAt = UtcNow

3. احفظ في DB

4. امسح من Cache:
   - DriverTripKey(driverId)
   - LocationKey(tripId)
```

---

### UpdateLocationAsync

```csharp
Task UpdateLocationAsync(int tripId, double lat, double lng, int driverId, CancellationToken ct)
```

**الخطوات:**

```
1. [FAST PATH] تحقق من الـ Cache:
   cache[DriverTripKey(driverId)] == tripId ؟
   ├── نعم → تخطى الـ DB تماماً ✅
   └── لا  → [FALLBACK] اجلب من DB + حدّث الـ Cache

2. حدّث الـ Cache:
   LocationKey(tripId) = { Latitude, Longitude, UtcNow }
```

> **لماذا هذا مهم؟**
> السائق يبعث UpdateLocation كل 3 ثوانٍ.
> 10 باصات × 20 تحديث/دقيقة = 200 عملية/دقيقة.
> بدون Cache → 200 SELECT على DB/دقيقة لمجرد التحقق.
> مع Cache → 0 DB operations في الحالة الطبيعية. 🚀

---

### AdjustAnonymousAsync

```csharp
Task<int> AdjustAnonymousAsync(int tripId, int delta, int driverId, CancellationToken ct)
```

**الخطوات:**

```
1. GetOwnedActiveTripAsync → تحقق من الملكية
2. trip.AdjustAnonymous(delta)
   - delta = +1 → ركب راكب
   - delta = -1 → نزل راكب
   - Math.Max(0, ...) → لا يصير سالب
3. احفظ في DB (هذه البيانات مهمة للإحصاءات)
4. أرجع العدد الجديد
```

---

### GetMyActiveTripAsync

```csharp
Task<GetTripModel?> GetMyActiveTripAsync(int driverId, CancellationToken ct)
```

**الخطوات:**

```
1. اجلب الرحلة من DB (مع Include)
   → أرجع null إذا ما في رحلة نشطة

2. اجلب الموقع من Cache (قد يكون null إذا السيرفر restart)

3. أرجع GetTripModel مع الموقع الحالي من Cache
```

---

### GetOwnedActiveTripAsync (Helper خاص)

```csharp
private async Task<TripEntity> GetOwnedActiveTripAsync(int tripId, int driverId, CancellationToken ct)
```

helper مشترك بين `EndTrip` و `AdjustAnonymous`:

```
1. GetActiveByIdAsync → NotFoundException إذا ما موجود
2. تحقق trip.DriverId == driverId → UnauthorizedException إذا لا
3. أرجع TripEntity
```

---

## 5. Presentation Layer — TrackingHub

**الملف:** `SoftPro.Wasilni.Presentation/Hubs/TrackingHub.cs`

### المصادقة

```csharp
[Authorize]  // كل المستخدمين يحتاجون JWT token صالح
```

الـ Hub يستخرج `driverId` من الـ JWT:
```csharp
int driverId = Context.User!.GetId();
```

### الـ Groups

```
trip-{tripId}   → السائق + الركاب المشتركين في هذه الرحلة
line-{lineId}   → الركاب اللي يبحثون عن باصات على خط معين
admin           → مشرفو النظام
```

---

### StartTrip

**يُستدعى من:** السائق

```
Client → Hub.StartTrip(busId)
         │
         ├── tripService.StartTripAsync(busId, driverId)
         │
         ├── Groups.AddToGroupAsync(connectionId, "trip-{tripId}")
         │
         ├── → Clients["line-{lineId}"].OnTripStarted(payload)  ← الركاب على الخط
         ├── → Clients["admin"].OnTripStarted(payload)           ← الأدمن
         └── → Clients.Caller.OnTripStarted(payload)             ← السائق نفسه (تأكيد)
```

**payload المُرسل:**
```json
{
  "id": 5,
  "busId": 3,
  "busPlate": "WAS-1001",
  "lineId": 1,
  "lineName": "Main Line",
  "status": "Active",
  "latitude": null,
  "longitude": null,
  "anonymousCount": 0,
  "startedAt": "2026-03-25T10:00:00Z"
}
```

---

### EndTrip

**يُستدعى من:** السائق

```
Client → Hub.EndTrip(tripId)
         │
         ├── tripService.EndTripAsync(tripId, driverId)
         │
         ├── → Clients["trip-{tripId}"].OnTripEnded({ tripId })  ← الركاب
         ├── → Clients["admin"].OnTripEnded({ tripId })           ← الأدمن
         └── Groups.RemoveFromGroupAsync(connectionId, "trip-{tripId}")
```

---

### UpdateLocation

**يُستدعى من:** السائق (كل 3 ثوانٍ)

```
Client → Hub.UpdateLocation(tripId, lat, lng)
         │
         ├── tripService.UpdateLocationAsync(...)
         │   └── Cache فقط، بدون DB write 🚀
         │
         ├── → Clients["trip-{tripId}"].OnLocationUpdated(payload)  ← الركاب
         └── → Clients["admin"].OnLocationUpdated(payload)           ← الأدمن
```

**payload المُرسل:**
```json
{
  "tripId": 5,
  "latitude": 33.5153,
  "longitude": 36.2785,
  "updatedAt": "2026-03-25T10:00:03Z"
}
```

---

### AdjustAnonymousPassenger

**يُستدعى من:** السائق

```
Client → Hub.AdjustAnonymousPassenger(tripId, delta)
         │   delta = +1 (ركب راكب) أو -1 (نزل راكب)
         │
         ├── tripService.AdjustAnonymousAsync(...) → newCount
         │   └── DB write (للإحصاءات)
         │
         └── → Clients["trip-{tripId}"].OnAnonymousCountUpdated({ tripId, count })
```

---

## 6. تدفق سيناريو السائق الكامل

```
┌─────────────────────────────────────────────────────────────┐
│  السائق يفتح التطبيق                                        │
│                                                             │
│  1. REST: GET /api/v1.0/trips/my-active                     │
│     └── يعرف هل عنده رحلة نشطة أم لا                        │
│                                                             │
│  2. SignalR: Connect (مع JWT token)                         │
│                                                             │
│  3. SignalR: StartTrip(busId)                               │
│     ← OnTripStarted { tripId, busPlate, lineName, ... }     │
│                                                             │
│  4. كل 3 ثوانٍ:                                             │
│     SignalR: UpdateLocation(tripId, lat, lng)               │
│     [بدون رد — fire and forget]                              │
│                                                             │
│  5. عند صعود راكب بدون تطبيق:                               │
│     SignalR: AdjustAnonymousPassenger(tripId, +1)           │
│     ← OnAnonymousCountUpdated { tripId, count: 1 }         │
│                                                             │
│  6. عند نزول الراكب:                                        │
│     SignalR: AdjustAnonymousPassenger(tripId, -1)           │
│     ← OnAnonymousCountUpdated { tripId, count: 0 }         │
│                                                             │
│  7. نهاية الرحلة:                                           │
│     SignalR: EndTrip(tripId)                                │
│     [لا يوجد رد للسائق — يُبلَّغ الركاب فقط]                │
└─────────────────────────────────────────────────────────────┘
```

---

## 7. معالجة الأخطاء

| الخطأ | السبب | الـ Exception |
|---|---|---|
| باص غير موجود | `busId` خاطئ | `NotFoundException` |
| ليس باصك | `bus.DriverId != driverId` | `UnauthorizedException` |
| الباص نشط مسبقاً | سائق آخر يقود نفس الباص | `AlreadyExistsException` |
| رحلة غير موجودة | `tripId` خاطئ أو منتهية | `NotFoundException` |
| ليست رحلتك | `trip.DriverId != driverId` | `UnauthorizedException` |

---

## 8. الـ Cache مقابل الـ DB

| العملية | DB | Cache | السبب |
|---|---|---|---|
| StartTrip | ✅ Write | ✅ Set | الرحلة تُحفظ دائماً |
| EndTrip | ✅ Write | ✅ Remove | تُغلق الرحلة في DB + تمسح الـ Cache |
| UpdateLocation | ❌ لا شيء | ✅ Set | بيانات مؤقتة + تحديث متكرر |
| AdjustAnonymous | ✅ Write | ❌ لا شيء | بيانات إحصائية مهمة |
| GetMyActiveTrip | ✅ Read | ✅ Read | DB للرحلة + Cache للموقع |

---

## 9. Client-Side Usage (JavaScript)

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/tracking", {
    accessTokenFactory: () => localStorage.getItem("token")
  })
  .build();

// استقبال الأحداث
connection.on("OnTripStarted",           (data) => console.log("Trip started", data));
connection.on("OnTripEnded",             (data) => console.log("Trip ended",   data));
connection.on("OnLocationUpdated",       (data) => updateMarker(data.latitude, data.longitude));
connection.on("OnAnonymousCountUpdated", (data) => updateCount(data.count));

await connection.start();

// استدعاء الـ Hub methods
await connection.invoke("StartTrip",                 busId);
await connection.invoke("EndTrip",                   tripId);
await connection.invoke("UpdateLocation",            tripId, lat, lng);
await connection.invoke("AdjustAnonymousPassenger",  tripId, +1);
```

---

## 10. ملاحظات مهمة

1. **CancellationToken في Hub**: لا تستخدم `CancellationToken` كـ parameter في Hub methods.
   بدلاً من ذلك استخدم `Context.ConnectionAborted` داخل الـ method.

2. **Group Subscription**: الركاب يجب أن يتصلوا بالـ Hub ويستدعوا `SubscribeToTrip(tripId)` ليستقبلوا التحديثات.

3. **Admin Group**: الـ Admin ينضم تلقائياً لـ `"admin"` group عند الاتصال (يمكن تطبيقه في `OnConnectedAsync`).

4. **Server Restart**: إذا أُعيد تشغيل السيرفر، يُفقد الـ Cache.
   السائق عند إعادة الاتصال و`StartTrip` → يُكتشف الـ Reconnect من DB ويُعاد بناء الـ Cache.
